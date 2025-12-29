using TopDown.Audio;
using TopDown.Shooting;
using UnityEngine;

[RequireComponent(typeof(PlayerAwareness))]
public class EnemyShooting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Animator muzzleFlashAnimator;
    [SerializeField] private AudioClip shotSound;

    [Header("Shooting")]
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private int poolSize = 6;

    [Header("Optional")]
    [SerializeField] private bool requireLineOfSight = false;
    [SerializeField] private LayerMask losObstacles;

    private PlayerAwareness awareness;
    private float fireCooldown;
    private Projectile[] pool;

    private int _playerLayerID;
    private int _playerLayerMask;

    private void Awake()
    {
        awareness = GetComponent<PlayerAwareness>();

        _playerLayerID = LayerMask.NameToLayer("Player");
        _playerLayerMask = LayerMask.GetMask("Player");

        if (projectilePrefab != null && poolSize > 0)
        {
            pool = new Projectile[poolSize];

            for (int i = 0; i < poolSize; i++)
            {
                // Tạo bản sao đạn sẵn trong bộ nhớ
                var go = Instantiate(projectilePrefab.gameObject, transform.position, Quaternion.identity);
                go.SetActive(false);
                // Lưu tham chiếu Component Projectile vào mảng
                pool[i] = go.GetComponent<Projectile>();
            }
        }
    }

    private void Update()
    {
        if (projectilePrefab == null || firePoint == null || awareness == null) return;
        if (!awareness.AwareOfPlayer) return;

        // Debugger for LOS
        if (requireLineOfSight)
        {
            Vector2 startPos = firePoint.position;
            Vector2 directionToPlayer = awareness.DirectionToPlayer.normalized;

            float maxDistance = 100f;

            // Chỉ kiểm tra va chạm với các lớp vật cản đã chọn và lớp Player
            LayerMask mask = losObstacles | _playerLayerMask;
            RaycastHit2D hit = Physics2D.Raycast(startPos, directionToPlayer, maxDistance, mask);

            if (hit.collider != null)
            {
                //Kiểm tra xem gameObject đầu tiên trúng tia Raycast có phải là Player
                bool hitPlayer = hit.collider.gameObject.layer == _playerLayerID;

                Debug.DrawRay(startPos, directionToPlayer * hit.distance,
                              hitPlayer ? Color.green : Color.red, 0.05f);
            }
            else
            {
                Debug.DrawRay(startPos, directionToPlayer * 5f, Color.yellow, 0.05f);
            }

            // Nếu tia Raycast bị chặn bởi layer khác hoặc không thấy Player thì không bắn
            if (hit.collider == null || hit.collider.gameObject.layer != _playerLayerID)
                return;
        }

        //quay nòng súng về Player
        Vector2 dir = awareness.DirectionToPlayer.normalized;
        firePoint.up = -dir;

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            Fire();
            fireCooldown = 1f / fireRate;
        }
    }

    private void Fire()
    {
        Projectile p = GetPooledProjectile();
        if (p == null)
        {
            //tạo đạn mới nếu hết Pool
            var go = Instantiate(projectilePrefab.gameObject, firePoint.position, firePoint.rotation);
            p = go.GetComponent<Projectile>();
        }
        else
        {
            //đưa đạn cũ về vị trí và Active nó
            p.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);
            p.gameObject.SetActive(true);
        }

        p.ShootBullet(firePoint);
        muzzleFlashAnimator.SetTrigger("shoot");
        SoundManager.Instance?.PlaySound(shotSound);
    }

    private Projectile GetPooledProjectile()
    {
        if (pool == null) return null;
        for (int i = 0; i < pool.Length; i++)
        {
            // Tìm viên đạn hiện đang inactive
            if (pool[i] != null && !pool[i].gameObject.activeInHierarchy)
                return pool[i];
        }
        return null;
    }
}
