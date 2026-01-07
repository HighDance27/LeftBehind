using TopDown.Audio;
using UnityEngine;
namespace TopDown.Shooting
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [Header("Movement Stats")]
        [SerializeField] private float speed;
        [SerializeField] private float lifetime;

        [Header("Combat")]
        [SerializeField] private int damage = 10;

        private Rigidbody2D body;
        private float lifeTimer;

        [SerializeField] private string wallLayerName = "Wall";
        private int wallLayer;

        [Header("Particle System")]
        [SerializeField] ParticleSystem enemyHitEffect;
        [SerializeField] ParticleSystem wallHitEffect;
        [SerializeField] ParticleSystem destroyableHitEffect;

        Vector3 startPos;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            wallLayer = LayerMask.NameToLayer(wallLayerName);
        }

        private void OnEnable()
        {
            startPos = GetComponent<Transform>().position;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            //tìm HealthController của vật thể bị va chạm hoặc cha của nó
            var health = collision.GetComponent<HealthController>()
                         ?? collision.GetComponentInParent<HealthController>();

            Vector2 lookDirection = startPos - collision.transform.position;
            bool isDestroyable = collision.CompareTag("Destroyable");

            //nếu là Destroyable
            if (isDestroyable)
            {
                ParticleSystem ps = Instantiate(destroyableHitEffect, gameObject.transform.position, Quaternion.LookRotation(lookDirection));
                Destroy(ps.gameObject, 1f);
                if (health != null)
                    health.TakeDamage(damage);

                gameObject.SetActive(false);
                return;
            }

            if (health != null)
            {
                ParticleSystem particleSystem;
                if (health.CurrentHealth == -1 && health.MaximumHealth == -1)
                {
                    SoundManager.Instance.PlaySound(health.wallSFX);
                    particleSystem = Instantiate(wallHitEffect, gameObject.transform.position, Quaternion.LookRotation(lookDirection));
                    Destroy(particleSystem.gameObject, 1);
                    gameObject.SetActive(false);
                }

                if (health.IsInvincible)
                {
                    gameObject.SetActive(false);
                    return;
                }

                if (health.CurrentHealth > 0)
                {
                    particleSystem = Instantiate(enemyHitEffect, gameObject.transform.position, Quaternion.LookRotation(lookDirection));
                    Destroy(particleSystem.gameObject, 1);
                    health.TakeDamage(damage);
                    gameObject.SetActive(false);
                    return;
                }
            }

            if (collision.gameObject.layer == wallLayer)
            {
                ParticleSystem particleSystem = Instantiate(wallHitEffect, gameObject.transform.position, Quaternion.LookRotation(lookDirection));
                Destroy(particleSystem.gameObject, 1);
                gameObject.SetActive(false);
            }
        }

        public void ShootBullet()
        {
            lifeTimer = 0f;
            body.linearVelocity = Vector2.zero;

            // Tính hướng bay dựa trên góc quay cuối cùng
            Vector2 dir = -(Vector2)transform.up;

            body.AddForce(dir * speed, ForceMode2D.Impulse);
        }

        public void BossShoot()
        {
            lifeTimer = 0;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            gameObject.SetActive(true);
            Vector2 dir = -transform.up;
            body.AddForce(dir * speed, ForceMode2D.Impulse);
        }

        private void Update()
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer > lifetime)
                gameObject.SetActive(false);
        }

    }
}
