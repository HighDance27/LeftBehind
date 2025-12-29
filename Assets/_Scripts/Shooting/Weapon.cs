using UnityEngine;
using UniRx;
using TopDown.Audio;
using System.Collections;
namespace TopDown.Shooting
{
    public class Weapon : MonoBehaviour
    {
        [Header("Cooldown")]
        [SerializeField] public float fireRateCooldown = 0.25f;
        private float cooldownTimer;

        [SerializeField] private bool canSpray = true;
        [SerializeField] private float reloadTime = 1f;
        public bool isReloading = false;

        [SerializeField] private float perShellReloadTime = 0.6f; //for shotgun
        [SerializeField] private bool isShotgun = false;

        private Coroutine reloadCoroutine;

        [Header("References")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform[] firePoints;
        [SerializeField] private Animator muzzleFlashAnimator;
        private ShotgunPumpSFX shotgunPumpSFX;
        public Animator crosshairAnim;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip shotSound;
        [SerializeField] private AudioClip reloadSound;
        [SerializeField] private AudioClip emptySound;
        private bool emptySoundPlayed;

        [Header("Ammo")]
        [SerializeField] public int initialAmmo;
        [SerializeField] public int clipSize;

        [SerializeField] private int poolSize = 30;
        private Projectile[] pool;

        private bool isHolding;

        //AmmoCounter tự động cập nhật khi Value thay đổi
        public IntReactiveProperty TotalAmmo { get; private set; } = new IntReactiveProperty(0);
        public IntReactiveProperty CurrentAmmoInClip { get; private set; } = new IntReactiveProperty(0);

        private void Awake()
        {
            TotalAmmo.Value = initialAmmo;
            CurrentAmmoInClip.Value = Mathf.Min(clipSize, initialAmmo);

            cooldownTimer = fireRateCooldown;
            shotgunPumpSFX = GetComponent<ShotgunPumpSFX>();

            if (bulletPrefab != null && poolSize > 0)
            {
                pool = new Projectile[poolSize];
                for (int i = 0; i < pool.Length; i++)
                {
                    GameObject go = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                    go.SetActive(false);
                    pool[i] = go.GetComponent<Projectile>();
                }
            }
        }

        private void Update()
        {
            cooldownTimer += Time.deltaTime;

            //Nếu súng tự động và người chơi giữ nút bắn
            if (canSpray && isHolding)
            {
                TryShoot();
            }
        }

        public void AddAmmo(int amount)
        {
            if (amount <= 0) return;
            TotalAmmo.Value += amount;
        }

        public void StartShooting()
        {
            emptySoundPlayed = false;
            if (canSpray)
            {
                isHolding = true;
                //Gọi TryShoot cho viên đầu
                TryShoot();

            }
            else
            {
                //Gọi hàm này cho mỗi lần nhấn
                TryShoot();
            }
        }

        public void StopShooting()
        {
            isHolding = false;
        }

        private void TryShoot()
        {
            if (cooldownTimer < fireRateCooldown) return;
            if (isReloading)
            {
                if (isShotgun && CurrentAmmoInClip.Value > 0)
                {
                    //Ngắt quá trình nhét đạn để bắn tiếp
                    CancelReload();
                }
                else
                {
                    return;
                }
            }

            Shoot();
            crosshairAnim.SetTrigger("Shoot");
        }

        private Projectile GetPooledBullet()
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
        public void Shoot()
        {
            if (UIManager.IsCleared) return;
            if (UIManager.IsPaused) return;
            if (TimelineController.IsCutscene) return;
            if (isReloading) return;

            if (CurrentAmmoInClip.Value <= 0)
            {
                //chạy âm thanh 1 lần duy nhất, kể cả giữ chuột
                if (!emptySoundPlayed)
                {
                    SoundManager.Instance?.PlaySound(emptySound);
                    emptySoundPlayed = true;
                }
                return;
            }

            //bắn từ các điểm firePoints
            foreach (Transform firePoint in firePoints)
            {
                Projectile p = GetPooledBullet();
                if (p == null)
                {
                    //hết pool
                    GameObject go = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                    p = go.GetComponent<Projectile>();
                }
                else
                {
                    p.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);
                    p.gameObject.SetActive(true);
                }
                p.ShootBullet(firePoint);
            }

            muzzleFlashAnimator.SetTrigger("shoot");
            cooldownTimer = 0;
            CurrentAmmoInClip.Value--;

            SoundManager.Instance?.PlaySound(shotSound); //Nếu Instance không null mới chạy, không thì ko làm gì

            //Nếu có gán component ShotgunPumpSFX
            var pump = GetComponent<ShotgunPumpSFX>();
            pump?.TriggerPump();
        }

        private void CancelReload()
        {
            if (reloadCoroutine != null)
            {
                StopCoroutine(reloadCoroutine);
                reloadCoroutine = null;
            }
            isReloading = false;
        }

        public void Reload()
        {
            if (isReloading) return;
            if (shotgunPumpSFX != null && shotgunPumpSFX.isPumping) return;
            if (TotalAmmo.Value <= 0) return;
            if (CurrentAmmoInClip.Value >= clipSize) return;

            isReloading = true;

            if (isShotgun)
                reloadCoroutine = StartCoroutine(ShotgunReloadRoutine());
            else
                reloadCoroutine = StartCoroutine(ReloadRoutine());
        }

        private IEnumerator ReloadRoutine()
        {
            SoundManager.Instance?.PlaySound(reloadSound);

            yield return new WaitForSeconds(reloadTime);

            int missingAmmo = clipSize - CurrentAmmoInClip.Value;
            int reloadAmmo = Mathf.Min(missingAmmo, TotalAmmo.Value);

            CurrentAmmoInClip.Value += reloadAmmo;
            TotalAmmo.Value -= reloadAmmo;

            isReloading = false;
            reloadCoroutine = null;
        }

        private IEnumerator ShotgunReloadRoutine()
        {
            while (CurrentAmmoInClip.Value < clipSize && TotalAmmo.Value > 0)
            {
                SoundManager.Instance?.PlaySound(reloadSound);

                yield return new WaitForSeconds(perShellReloadTime);

                CurrentAmmoInClip.Value++;
                TotalAmmo.Value--;

            }

            isReloading = false;
            reloadCoroutine = null;
        }

        private void OnDisable()
        {
            if (isReloading)
            {
                isReloading = false;

                if (reloadCoroutine != null)
                {
                    StopCoroutine(reloadCoroutine);
                    reloadCoroutine = null;
                }
            }

            isHolding = false;
        }
    }
}
