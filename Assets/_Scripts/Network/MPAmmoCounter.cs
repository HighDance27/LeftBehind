using System;
using UniRx;
using TMPro;
using UnityEngine;
using TopDown.Shooting;

namespace TopDown.UI
{
    public class MPAmmoCounter : MonoBehaviour
    {
        private readonly CompositeDisposable weaponSubscriptions = new CompositeDisposable();
        private IDisposable equippedWeaponSubscription;

        [Header("References")]
        [SerializeField] private TextMeshProUGUI ammoCounterText;

        private MPWeaponSystem weaponSystem;
        private int ammoInClip;
        private int totalAmmo;

        //Khi UI bật lại, kiểm tra xem đã có weaponSystem chưa, nếu có thì đăng ký lại
        private void OnEnable()
        {
            if (weaponSystem != null)
            {
                InitListeners();
            }
        }

        private void OnDisable()
        {
            ClearAllSubs();
        }

        public void SetWeaponSystem(MPWeaponSystem ws)
        {
            if (ws == weaponSystem && ws != null) return;
            weaponSystem = ws;

            ClearAllSubs();

            if (weaponSystem == null)
            {
                UpdateAmmoCounter(0, 0);
                return;
            }
            InitListeners();
        }

        private void InitListeners()
        {
            ClearAllSubs();
            //lắng nghe đổi súng
            equippedWeaponSubscription = weaponSystem
                .ObserveEveryValueChanged(x => x.EquippedWeapon)
                .Subscribe(newWeapon =>
                {
                    weaponSubscriptions.Clear(); //xóa listener cũ

                    if (newWeapon != null)
                        SubscribeToWeapon(newWeapon); //lắng nghe mới
                    else
                        UpdateAmmoCounter(0, 0);
                });

            // Nếu đang cầm sẵn súng rồi thì lắng nghe luôn (trường hợp init lần đầu)
            if (weaponSystem.EquippedWeapon != null)
            {
                SubscribeToWeapon(weaponSystem.EquippedWeapon);
            }
        }

        private void SubscribeToWeapon(MPWeapon weapon)
        {
            weapon.CurrentAmmoInClip
                .ObserveEveryValueChanged(p => p.Value)
                .Subscribe(value =>
                {
                    ammoInClip = value;
                    UpdateAmmoCounter(ammoInClip, totalAmmo);
                })
                .AddTo(weaponSubscriptions);

            weapon.TotalAmmo
                .ObserveEveryValueChanged(p => p.Value)
                .Subscribe(value =>
                {
                    totalAmmo = value;
                    UpdateAmmoCounter(ammoInClip, totalAmmo);
                })
                .AddTo(weaponSubscriptions);

            // Cập nhật UI ngay lập tức
            UpdateAmmoCounter(weapon.CurrentAmmoInClip.Value, weapon.TotalAmmo.Value);
        }

        private void UpdateAmmoCounter(int currentAmmo, int total)
        {
            if (ammoCounterText == null) return;

            // Knife shows 0/0
            if (weaponSystem != null && weaponSystem.EquippedWeapon != null &&
                weaponSystem.EquippedWeapon.GetComponent<MPKnifeAttack>())
            {
                ammoCounterText.text = "0/0";
                return;
            }

            ammoCounterText.text = $"{currentAmmo}/{total}";
        }

        private void ClearAllSubs()
        {
            weaponSubscriptions.Clear();
            if (equippedWeaponSubscription != null)
            {
                equippedWeaponSubscription.Dispose();
                equippedWeaponSubscription = null;
            }
        }
    }
}
