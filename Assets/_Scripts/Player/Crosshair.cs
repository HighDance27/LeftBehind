using TopDown.Shooting;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private WeaponSystem weaponSystem;
    private Camera mainCamera;

    [Header("Sprites")]
    [SerializeField] private Sprite normalCrosshair;
    [SerializeField] private Sprite reloadCrosshair;

    private void Awake()
    {
        //Tìm SpriteRenderer ở object con, cho dù có đang tắt
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        weaponSystem = FindObjectOfType<WeaponSystem>();
        mainCamera = Camera.main;
    }

    void Start()
    {
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        Vector2 cursorPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        transform.position = cursorPos;

        if (UIManager.IsPaused || UIManager.IsCleared)
        {
            spriteRenderer.enabled = false;
            Cursor.visible = true;
            return;
        }

        spriteRenderer.enabled = true;
        Cursor.visible = false;

        Weapon currentWeapon;

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
