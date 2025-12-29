using System.Collections;
using UnityEngine;
using TopDown.Shooting;
using TopDown.Audio;

[RequireComponent(typeof(PlayerAwareness))]
public class BossShooting : MonoBehaviour
{
    public enum FireMode
    {
        Shotgun,
        MachineGun
    }

    [Header("References")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Animator muzzleFlashAnimator;
    [SerializeField] private AudioClip shotSound;

    [Header("General Shooting")]
    [SerializeField] private float cooldown = 2.5f;
    [SerializeField] private FireMode fireMode = FireMode.Shotgun;
    [SerializeField] private int poolSize = 30;
    private Projectile[] pool;

    [Header("Shotgun Settings")]
    [SerializeField] private int shotgunBullets = 5;
    [SerializeField] private float shotgunSpreadAngle = 15f;

    [Header("Machine Gun Settings")]
    [SerializeField] private int machineGunBullets = 10;
    [SerializeField] private float machineGunInterval = 0.05f;

    private PlayerAwareness awareness;
    private float fireTimer;
    private bool isFiringBurst;

    private void Awake()
    {
        awareness = GetComponent<PlayerAwareness>();
        if (projectilePrefab != null && poolSize > 0)
        {
            pool = new Projectile[poolSize];

            for (int i = 0; i < pool.Length; i++)
            {
                var go = Instantiate(projectilePrefab.gameObject, transform.position, Quaternion.identity);
                go.SetActive(false);
                pool[i] = go.GetComponent<Projectile>(); // Lưu tham chiếu Component Projectile vào mảng
            }
        }
    }

    private void Update()
    {
        if (projectilePrefab == null || firePoint == null) return;
        if (awareness == null || !awareness.AwareOfPlayer) return;

        // Aim at player
        Vector2 dir = awareness.DirectionToPlayer.normalized;
        firePoint.up = -dir;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f && !isFiringBurst)  //MachineGun use Coroutine
        {
            switch (fireMode)
            {
                //đổi súng & bắn
                case FireMode.Shotgun:
                    FireShotgun();
                    fireTimer = cooldown;
                    break;

                case FireMode.MachineGun:
                    StartCoroutine(FireMachineGunBurst());
                    fireTimer = cooldown;
                    break;
            }
            RandomFireMode();
        }
    }

    private Projectile GetPooledProjectile()
    {
        if (pool == null) return null;
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i] != null && !pool[i].gameObject.activeInHierarchy)
                return pool[i];
        }
        return null;
    }

    private void FireShotgun()
    {
        //Tính điểm giữa của số lượng đạn để chia đều góc sang 2 bên
        float half = (shotgunBullets - 1) * 0.5f;

        for (int i = 0; i < shotgunBullets; i++)
        {
            //Tính góc lệch cho viên đạn thứ i dựa trên độ tản
            // (i - half) sẽ cho ra dải số đối xứng, vd: -1, 0, 1
            float angleOffset = (i - half) * shotgunSpreadAngle;

            // Tính hướng xoay riêng cho từng viên đạn Shotgun
            Quaternion bulletRotation = firePoint.rotation * Quaternion.Euler(0, 0, angleOffset);

            Projectile p = GetPooledProjectile();
            if (p == null)
            {
                var go = Instantiate(projectilePrefab.gameObject, firePoint.position, bulletRotation);
                p = go.GetComponent<Projectile>();
            }
            else
            {
                p.transform.SetPositionAndRotation(firePoint.position, bulletRotation);
                p.gameObject.SetActive(true);
            }
            p.BossShoot();
        }

        PlayMuzzleAndSound();
    }

    private IEnumerator FireMachineGunBurst()
    {
        isFiringBurst = true;

        for (int i = 0; i < machineGunBullets; i++)
        {
            Projectile p = GetPooledProjectile();
            if (p == null)
            {
                var go = Instantiate(projectilePrefab.gameObject, firePoint.position, firePoint.rotation);
                p = go.GetComponent<Projectile>();
            }

            else
            {
                p.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);
                p.gameObject.SetActive(true);
            }

            p.BossShoot();
            PlayMuzzleAndSound();

            // Kiểm tra nếu chưa phải viên đạn cuối cùng thì mới tạm dừng
            if (i < machineGunBullets - 1)
                //Tạo khoảng trễ giữa các viên đạn để thấy được độ liên thanh
                yield return new WaitForSeconds(machineGunInterval);
        }

        isFiringBurst = false;
    }

    private void PlayMuzzleAndSound()
    {
        muzzleFlashAnimator.SetTrigger("shoot");
        SoundManager.Instance?.PlaySound(shotSound);
    }

    private void RandomFireMode()
    {
        //0,1 thì luôn trả về 0 nên để 2
        int mode = Random.Range(0, 2);

        fireMode = (mode == 0)
            ? FireMode.Shotgun
            : FireMode.MachineGun;
    }
}
