using System.Collections;
using TopDown.Audio;
using TopDown.Shooting;
using UnityEngine;
using Photon.Pun;

public class MPWeaponSystem : MonoBehaviour
{
    public MPWeapon[] weapons;
    public MPWeapon EquippedWeapon;
    [SerializeField] private AudioClip switchSound;

    private PlayerHUD playerHUD;

    private bool canSwitchWeapon = true;
    private PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        playerHUD = FindObjectOfType<PlayerHUD>();
        EquippedWeapon = weapons[0];
        for (int i = 1; i < weapons.Length; i++)
        {
            weapons[i].gameObject.SetActive(false);
        }
    }

    public void EquipGun(int gunToEquip)
    {
        if (!canSwitchWeapon) return;

        if (PhotonNetwork.IsConnected && photonView != null)
        {
            if (!photonView.IsMine) return;

            photonView.RPC(
                "RPC_EquipGun",
                RpcTarget.All, //gọi cho tất cả máy khác để họ thấy mình đổi súng
                gunToEquip
            );
        }
        else
        {
            //gọi trực tiếp nếu không kết nối 
            DoEquipGun(gunToEquip);
        }
    }

    [PunRPC]
    private void RPC_EquipGun(int gunToEquip)
    {
        DoEquipGun(gunToEquip);
    }

    private void DoEquipGun(int gunToEquip)
    {
        if (!canSwitchWeapon) return;
        if (gunToEquip <= 0 || gunToEquip > weapons.Length) return;

        SoundManager.Instance?.PlaySound(switchSound);
        StartCoroutine(WeaponSwitchCooldown()); //Start cooldown

        foreach (MPWeapon weapon in weapons)
        {
            //tắt súng không dùng
            weapon.gameObject.SetActive(false);
        }

        EquippedWeapon = weapons[gunToEquip - 1]; //chọn súng dựa vào index
        EquippedWeapon.gameObject.SetActive(true);

        if (photonView == null || !PhotonNetwork.IsConnected || photonView.IsMine)
        {
            if (playerHUD != null)
                playerHUD.UpdateWeaponIcon(gunToEquip);
            else
                Debug.LogWarning("[WeaponSystem] No UIManager or PlayerHUD available to update weapon icon!");
        }
    }

    private IEnumerator WeaponSwitchCooldown()
    {
        canSwitchWeapon = false;
        yield return new WaitForSeconds(0.5f);
        canSwitchWeapon = true;
    }
}
