using Photon.Pun;
using TopDown.Shooting;
using UnityEngine;

public class MPCrosshair : MonoBehaviourPun
{
    private SpriteRenderer spriteRenderer;
    private MPWeaponSystem weaponSystem;

    [Header("Sprites")]
    [SerializeField] private Sprite normalCrosshair;
    [SerializeField] private Sprite reloadCrosshair;

    private void Awake()
    {
        //tìm component cho dù có đang tắt
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        weaponSystem = FindObjectOfType<MPWeaponSystem>();
    }

    void Start()
    {
        Cursor.visible = false;
    }

    void Update()
    {
        Vector2 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = cursorPos;

        if (PlayerHUD.IsPaused || PlayerHUD.IsCleared || PlayerHUD.IsGameOver)
        {
            spriteRenderer.enabled = false;
            Cursor.visible = true;
            return;
        }

        if (photonView.IsMine)
        {
            spriteRenderer.enabled = true;
            Cursor.visible = false;
        }
        MPWeapon currentWeapon;

        if (weaponSystem != null)
            currentWeapon = weaponSystem.EquippedWeapon;  // if found
        else
            currentWeapon = null;

        if (currentWeapon != null && currentWeapon.isReloading)
            spriteRenderer.sprite = reloadCrosshair;
        else
            spriteRenderer.sprite = normalCrosshair;
    }
}
