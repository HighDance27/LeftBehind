using UnityEngine;
using UniRx;
using TopDown.Audio;
using System.Collections;
using Photon.Pun;
using System.Collections.Generic;
namespace TopDown.Shooting
{
    public class MPWeapon : MonoBehaviour
    {
        private PhotonView photonView;

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
        [SerializeField] public GameObject realBulletPrefab;
        [SerializeField] public GameObject fakeBulletPrefab;
        [SerializeField] public Transform[] firePoints;
        [SerializeField] private Animator muzzleFlashAnimator;
        [SerializeField] public int weaponIndex;
        private MPPlayerController playerController;
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

        private bool isHolding;

        private List<MPProjectile> realBulletPool = new List<MPProjectile>();

        //tự cập nhật đạn lên ui
        public IntReactiveProperty TotalAmmo { get; private set; } = new IntReactiveProperty(0);
        public IntReactiveProperty CurrentAmmoInClip { get; private set; } = new IntReactiveProperty(0);

        private void Awake()
        {
            photonView = GetComponentInParent<PhotonView>();
            playerController = GetComponentInParent<MPPlayerController>();

            TotalAmmo.Value = initialAmmo;
            CurrentAmmoInClip.Value = Mathf.Min(clipSize, initialAmmo);

            cooldownTimer = fireRateCooldown;
            shotgunPumpSFX = GetComponent<ShotgunPumpSFX>();
        }
        private void Update()
        {
            cooldownTimer += Time.deltaTime;

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
                TryShoot();

            }
            else
            {
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

        public void Shoot()
        {
            if (PlayerHUD.IsCleared) return;
            if (PlayerHUD.IsPaused) return;
            if (isReloading) return;
            if (CurrentAmmoInClip.Value <= 0)
            {
                // chạy sfc 1 lần khi giữ
                if (!emptySoundPlayed)
                {
                    SoundManager.Instance?.PlaySound(emptySound);
                    emptySoundPlayed = true;
                }
                return;
            }

            for (int i = 0; i < firePoints.Length; i++)
            {
                Transform fp = firePoints[i];

                //máy khác tự spawn đạn fake
                playerController.photonView.RPC("RPC_SpawnFakeBullet", RpcTarget.Others, fp.position, fp.rotation);

                // đạn thật tự xử lý ở máy local
                if (!PhotonNetwork.IsConnected || photonView.IsMine)
                {
                    MPProjectile realBullet = GetBulletFromPool();
                    //nếu dùng đạn cũ từ pool, nếu ID không khớp, đạn có thể va chạm ngay với Collider của người bắn
                    realBullet.OwnerID = photonView.ViewID;
                    realBullet.ShootBullet(fp);
                }
            }

            muzzleFlashAnimator.SetTrigger("shoot");
            cooldownTimer = 0;
            CurrentAmmoInClip.Value--;

            SoundManager.Instance?.PlaySound(shotSound);

            var pump = GetComponent<ShotgunPumpSFX>();
            pump?.TriggerPump();
        }

        private MPProjectile GetBulletFromPool()
        {
            // Tìm trong list xem có viên nào đang tắt không
            foreach (var bullet in realBulletPool)
            {
                if (!bullet.gameObject.activeInHierarchy)
                {
                    return bullet;
                }
            }

            //Nếu không có hoặc hết, tạo mới
            GameObject newBulletObj = Instantiate(realBulletPrefab);
            MPProjectile newProjectile = newBulletObj.GetComponent<MPProjectile>();

            newBulletObj.SetActive(false);

            realBulletPool.Add(newProjectile);
            return newProjectile;
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
            // Khi đổi súng, reset trạng thái nạp đạn
            if (isReloading)
            {
                isReloading = false;

                //Dừng Coroutine nạp đạn ngay 
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
