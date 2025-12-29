using UnityEngine;
using TopDown.Audio;
using Photon.Pun;
using System.Collections;

namespace TopDown.Shooting
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MPProjectile : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float speed = 5f;
        [SerializeField] private float lifetime = 2f;

        [Header("Damage")]
        [SerializeField] private int damage = 10;

        [Header("Accuracy")]
        [Tooltip("10 = perfect aim, 1 = very inaccurate")]
        [Range(1f, 10f)][SerializeField] private float accuracyRating = 10f;
        [Tooltip("Max deviation (degrees) when accuracy = 1/10")]
        [SerializeField] private float maxSpreadDegrees = 10f;

        [Header("Layers & Effects")]
        [SerializeField] private string wallLayerName = "Wall";
        [SerializeField] private ParticleSystem enemyHitEffect;
        [SerializeField] private ParticleSystem wallHitEffect;
        [SerializeField] private ParticleSystem destroyableHitEffect;

        private Rigidbody2D _rb;
        private float _lifeTimer;
        private int _wallLayer;
        private Vector3 _startPos;

        public bool IsPlayerBullet = true;
        public int OwnerID; //bullet owner

        private Coroutine _launchCoroutine;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _wallLayer = LayerMask.NameToLayer(wallLayerName);
            _startPos = transform.position;
        }

        private void OnEnable()
        {
            _lifeTimer = 0f;
        }

        private void Update()
        {
            _lifeTimer += Time.deltaTime;
            if (_lifeTimer > lifetime)
                gameObject.SetActive(false);
        }

        public void ShootBullet(Transform shootPoint)
        {
            ShootBullet(shootPoint.position, shootPoint.rotation, 0f);
        }

        public void ShootBullet(Vector3 position, Quaternion rotation, float startDelay = 0f)
        {
            //reset trạng thái
            _lifeTimer = 0f;
            _rb.linearVelocity = Vector2.zero;

            transform.position = position;

            //apply độ tản dựa vào độ chính xác
            float spread = Mathf.Lerp(maxSpreadDegrees, 0f, accuracyRating / 10f);
            float zOffset = Random.Range(-spread * 0.5f, spread * 0.5f);

            //Lấy góc quay của súng cộng thêm góc lệch random
            Quaternion shotRot = rotation * Quaternion.Euler(0f, 0f, zOffset);
            transform.rotation = shotRot;

            gameObject.SetActive(true);
            // Nếu có Coroutine cũ đang chạy (ví dụ dùng lại từ pool quá nhanh), dừng nó
            if (_launchCoroutine != null) StopCoroutine(_launchCoroutine);

            // Nếu cần delay thì chạy Coroutine, không thì bắn ngay
            if (startDelay > 0f)
            {
                _launchCoroutine = StartCoroutine(DelayedLaunch(startDelay, shotRot));
            }
            else
            {
                ApplyVelocity(shotRot);
            }
        }

        private IEnumerator DelayedLaunch(float delay, Quaternion rotation)
        {
            // Đợi 1 khoảng thời gian (Master đứng yên chờ Client bắt kịp)
            yield return new WaitForSeconds(delay);
            ApplyVelocity(rotation);
        }

        private void ApplyVelocity(Quaternion rotation)
        {
            _startPos = transform.position; // Cập nhật lại vị trí bắt đầu tính từ lúc bay
            Vector2 dir = -(Vector2)(rotation * Vector3.up);
            _rb.linearVelocity = dir * speed;
        }

        public void SetAccuracy(float rating)
        {
            accuracyRating = Mathf.Clamp(rating, 1f, 10f);
        }

        private void Deactivate()
        {
            // reset to pool
            if (_launchCoroutine != null) StopCoroutine(_launchCoroutine);
            _rb.linearVelocity = Vector2.zero;
            gameObject.SetActive(false);
        }

        private void SpawnEffect(ParticleSystem effect, Vector3 pos, Quaternion rot)
        {
            if (effect == null) return;

            var ps = Instantiate(effect, pos, rot);
            Destroy(ps.gameObject, ps.main.duration);
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            //ngăn không cho đạn tự bay vào người bắn
            PhotonView hitView = col.GetComponentInParent<PhotonView>();
            if (hitView != null && hitView.ViewID == OwnerID)
            {
                return;
            }

            if (col.GetComponent<InvisibleWall>() != null ||
                   col.GetComponentInParent<InvisibleWall>() != null)
            {
                return;
            }

            // tìm MPHealth khi va chạm
            var mpHealth = col.GetComponent<MPHealthController>() ?? col.GetComponentInParent<MPHealthController>();
            //tạo hướng xoay ngược khi va cham cho vfx
            Vector2 lookDir = _startPos - col.transform.position;

            //Destroyable
            if (col.CompareTag("Destroyable"))
            {
                SpawnEffect(destroyableHitEffect, transform.position, Quaternion.LookRotation(lookDir));

                if (mpHealth != null)
                {
                    mpHealth.TakeDamage(damage);
                }

                Deactivate();
                return;
            }

            //va chạm với vật có máu
            if (mpHealth != null)
            {
                if (mpHealth.CurrentHealth == -1 && mpHealth.MaximumHealth == -1)
                {
                    if (mpHealth.wallSFX)
                        SoundManager.Instance?.PlaySound(mpHealth.wallSFX);

                    SpawnEffect(wallHitEffect, transform.position, Quaternion.LookRotation(lookDir));
                    Deactivate();
                    return;
                }

                if (mpHealth.IsInvincible)
                {
                    Deactivate();
                    return;
                }

                if (mpHealth.CurrentHealth > 0)
                {
                    SpawnEffect(enemyHitEffect, transform.position, Quaternion.LookRotation(lookDir));
                    mpHealth.TakeDamage(damage);
                    Deactivate();
                    return;
                }
            }

            //layer wall
            if (col.gameObject.layer == _wallLayer)
            {
                SpawnEffect(wallHitEffect, transform.position, Quaternion.LookRotation(lookDir));
                Deactivate();
            }
        }
    }
}
