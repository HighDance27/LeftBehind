using UniRx;
using TMPro;
using UnityEngine;
using TopDown.Shooting;
namespace TopDown.UI
{
    public class AmmoCounter : MonoBehaviour
    {
        //container để chứa các event đã đăng ký
        private CompositeDisposable subscriptions = new CompositeDisposable();

        [Header("References")]
        [SerializeField] private TextMeshProUGUI ammoCounterText;
        [SerializeField] private WeaponSystem weaponSystem;

        private int ammoInClip;
        private int totalAmmo;

        private void OnEnable()
        {   //Theo dõi biến EquippedWeapon trong weaponSystem.   
            //mỗi lần đổi vũ khí, logic bên trong Subscribe sẽ chạy
            weaponSystem.ObserveEveryValueChanged(ws => ws.EquippedWeapon)
            .Subscribe(newWeapon =>
            {
                //xóa các đăng ký theo dõi số đạn của súng cũ
                subscriptions.Clear();

                //đăng ký theo dõi súng mới
                if (newWeapon != null)
                {
                    SubscribeToWeapon(newWeapon);
                }

            }).AddTo(this); //Gắn vòng đời của đăng ký này vào chính gameObject này
        }

        private void OnDisable()
        {
            // Khi UI bị tắt, hủy toàn bộ đăng ký để giải phóng tài nguyên
            subscriptions.Clear();
        }

        private void SubscribeToWeapon(Weapon weapon)
        {
            //theo dõi giá trị hiện tại trong băng đạn để tự cập nhật mỗi khi bắn, nạp đạn
            weapon.CurrentAmmoInClip.ObserveEveryValueChanged(property => property.Value)
                .Subscribe(value =>
                {
                    ammoInClip = value;
                    UpdateAmmoCounter(ammoInClip, totalAmmo);
                }).AddTo(subscriptions);

            weapon.TotalAmmo.ObserveEveryValueChanged(property => property.Value)
                .Subscribe(value =>
                {
                    totalAmmo = value;
                    UpdateAmmoCounter(ammoInClip, totalAmmo);
                }).AddTo(subscriptions);
        }

        private void UpdateAmmoCounter(int currentAmmo, int totalAmmo)
        {
            ammoCounterText.text = $"{currentAmmo}/{totalAmmo}";

            if (weaponSystem.EquippedWeapon.GetComponent<KnifeAttack>())
                ammoCounterText.text = $"{0}/{0}";
        }
    }
}
