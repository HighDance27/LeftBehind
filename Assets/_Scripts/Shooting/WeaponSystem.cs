using System.Collections;
using TopDown.Audio;
using TopDown.Shooting;
using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    public Weapon[] weapons;
    public Weapon EquippedWeapon;
    [SerializeField] private AudioClip switchSound;

    private UIManager uiManager;

    private bool canSwitchWeapon = true;

    void Start()
    {
        TryFindUI();
        EquippedWeapon = weapons[0];

        for (int i = 1; i < weapons.Length; i++)
        {
            weapons[i].gameObject.SetActive(false);
        }
        RestoreAllWeaponsAmmoFromPrefs();
        SaveAllWeaponsAmmoToPrefs();
    }

    public void SaveAllWeaponsAmmoToPrefs()
    {
        PlayerPrefs.SetInt("Ammo_Count", weapons.Length);
        for (int i = 0; i < weapons.Length; i++)
        {
            var w = weapons[i];
            if (w == null) continue;

            PlayerPrefs.SetInt($"Ammo_{i}_Total", w.TotalAmmo.Value);
            PlayerPrefs.SetInt($"Ammo_{i}_Clip", w.CurrentAmmoInClip.Value);
        }
        PlayerPrefs.Save();
    }

    private void RestoreAllWeaponsAmmoFromPrefs()
    {
        if (!PlayerPrefs.HasKey("Ammo_Count")) return;

        //Lấy số lượng súng đã lưu
        int count = PlayerPrefs.GetInt("Ammo_Count");

        for (int i = 0; i < weapons.Length && i < count; i++)
        {
            //Kiểm tra xem có dữ liệu đạn của súng thứ i này không
            if (!PlayerPrefs.HasKey($"Ammo_{i}_Total")) continue;

            var w = weapons[i];
            if (w == null) continue;

            w.TotalAmmo.Value = PlayerPrefs.GetInt($"Ammo_{i}_Total");
            w.CurrentAmmoInClip.Value = PlayerPrefs.GetInt($"Ammo_{i}_Clip");
        }

        //Xóa keys để không ghi đề dữ liệu cũ
        PlayerPrefs.DeleteKey("Ammo_Count");

        for (int i = 0; i < count; i++)
        {
            PlayerPrefs.DeleteKey($"Ammo_{i}_Total");
            PlayerPrefs.DeleteKey($"Ammo_{i}_Clip");
        }
        PlayerPrefs.Save();
    }

    public void RestoreFullAmmo()
    {
        foreach (var w in weapons)
        {
            if (w == null) continue;

            //Đặt lại đạn tương ứng với đạn khởi đầu của vũ khí đó
            var weapon = w.GetComponent<Weapon>();
            if (weapon == null) continue;

            weapon.TotalAmmo.Value = weapon.initialAmmo;
            weapon.CurrentAmmoInClip.Value = Mathf.Min(weapon.clipSize, weapon.initialAmmo);
        }

        SaveAllWeaponsAmmoToPrefs();
    }

    private void TryFindUI()
    {
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>(true);
    }

    public void EquipGun(int gunToEquip)
    {
        if (!canSwitchWeapon) return;
        if (gunToEquip > 0 && gunToEquip <= weapons.Length)
        {
            SoundManager.Instance?.PlaySound(switchSound);
            StartCoroutine(WeaponSwitchCooldown());

            foreach (Weapon weapon in weapons)
            {
                //Tắt tất cả súng không chọn
                weapon.gameObject.SetActive(false);
            }

            EquippedWeapon = weapons[gunToEquip - 1]; //Kích hoạt khẩu súng được chọn (chỉ số = số nhập vào - 1)
            EquippedWeapon.gameObject.SetActive(true);

            TryFindUI();
            uiManager.UpdateWeaponIcon(gunToEquip);
        }
    }

    private IEnumerator WeaponSwitchCooldown()
    {
        canSwitchWeapon = false;
        yield return new WaitForSeconds(0.5f);
        canSwitchWeapon = true;
    }
}
