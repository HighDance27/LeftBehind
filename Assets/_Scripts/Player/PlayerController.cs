using TopDown.Movement;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public WeaponSystem weapons;
    private Player playerMovement;
    private MultiMovement multiMovement;
    private KnifeAttack currentKnife;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string equipKnifeTrigger = "EquipKnife";

    private void Awake()
    {
        playerMovement = GetComponent<Player>();
        multiMovement = GetComponent<MultiMovement>();
        animator = GetComponent<Animator>();
    }

    public void Shoot()
    {
        weapons.EquippedWeapon.Shoot();
    }

    private void OnShoot(InputValue value)
    {
        bool isPressed = value.isPressed;

        if (currentKnife != null)
        {
            if (isPressed)
                currentKnife.StartAttack();
            else
                currentKnife.StopAttack();
            return;
        }

        var weapon = weapons.EquippedWeapon;
        if (weapon == null) return;
        if (isPressed)
        {
            weapon.StartShooting();
        }
        else
        {
            weapon.StopShooting();    //if canSpray = true
        }
    }


    public void Reload()
    {
        weapons.EquippedWeapon.Reload();
    }

    private void OnMove(InputValue value)
    {
        //Lấy giá trị từ input dưới dạng Vector2
        Vector2 input = value.Get<Vector2>();
        if (playerMovement != null)
        {
            playerMovement.HandleMoveInput(input);
        }
        else if (multiMovement != null)
        {
            multiMovement.HandleMoveInput(input);
        }
    }

    private void OnDodge(InputValue value)
    {
        if (!value.isPressed) return;

        if (playerMovement != null)
        {
            playerMovement.HandleDodgeInput();
        }
        else if (multiMovement != null)
        {
            multiMovement.HandleDodgeInput();
        }
    }
    // void Update()
    // {

    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         if (currentKnife != null) currentKnife.StartAttack();
    //         else
    //         {
    //             weapons.EquippedWeapon.StartShooting();
    //         }

    //     }

    //     if (Input.GetMouseButtonUp(0))
    //     {
    //         if (currentKnife != null) currentKnife.StopAttack();
    //         else weapons.EquippedWeapon.StopShooting();
    //     }
    // }

    private void OnReload(InputValue value)
    {
        if (value.isPressed)
        {
            weapons.EquippedWeapon.Reload();
        }
    }
    private void OnSwitch1(InputValue value)
    {
        if (value.isPressed)
        {
            weapons.EquipGun(1);
            currentKnife = null;
        }
    }

    private void OnSwitch2(InputValue value)
    {
        if (value.isPressed)
        {
            weapons.EquipGun(2);
            currentKnife = null;
        }
    }

    private void OnSwitch3(InputValue value)
    {
        if (value.isPressed)
        {
            weapons.EquipGun(3);
            currentKnife = null;
        }
    }

    private void OnSwitch4(InputValue value)
    {
        if (value.isPressed)
        {
            weapons.EquipGun(4);
            animator.SetTrigger(equipKnifeTrigger);
            currentKnife = GetComponentInChildren<KnifeAttack>();
        }
    }
}
