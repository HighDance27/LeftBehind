using Photon.Pun;
using TopDown.Shooting;
using UnityEngine;
using UnityEngine.UI;

public class MPCrosshair : MonoBehaviourPun
{
    private Image crosshairImage;
    private MPWeaponSystem weaponSystem;

    [Header("Sprites")]
    [SerializeField] private Sprite normalCrosshair;
    [SerializeField] private Sprite reloadCrosshair;

    private void Awake()
    {
        //tìm component cho dù có đang tắt
        crosshairImage = GetComponent<Image>();
        if (crosshairImage == null)
            crosshairImage = GetComponentInChildren<Image>(true);
        weaponSystem = FindObjectOfType<MPWeaponSystem>();
    }

    void Start()
    {
        Cursor.visible = false;
    }

    void Update()
    {
        transform.position = Input.mousePosition;

        if (PlayerHUD.IsPaused || PlayerHUD.IsCleared || PlayerHUD.IsGameOver)
        {
            if (crosshairImage) crosshairImage.enabled = false;
            Cursor.visible = true;
            return;
        }

        if (photonView.IsMine)
        {
            if (crosshairImage) crosshairImage.enabled = true;
            Cursor.visible = false;
        }
        else
        {
            // Tắt crosshair của người khác
            if (crosshairImage) crosshairImage.enabled = false;
            return;
        }

        MPWeapon currentWeapon = (weaponSystem != null) ? weaponSystem.EquippedWeapon : null;

        if (crosshairImage != null)
        {
            if (currentWeapon != null && currentWeapon.isReloading)
                crosshairImage.sprite = reloadCrosshair;
            else
                crosshairImage.sprite = normalCrosshair;
        }
    }
}
