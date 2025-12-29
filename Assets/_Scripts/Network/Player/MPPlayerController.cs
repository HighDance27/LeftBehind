using Photon.Pun;
using TopDown.Movement;
using UnityEngine;
using UnityEngine.InputSystem;

public class MPPlayerController : MonoBehaviour
{
    public MPWeaponSystem weapons;
    private MultiMovement multiMovement;
    private MPKnifeAttack currentKnife;
    private SpriteRenderer legsSprite;
    public GameObject fakeBulletPrefab;

    [SerializeField] private bool disableLeg = false;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string equipKnifeTrigger = "EquipKnife";

    public PhotonView photonView;

    private void Awake()
    {
        multiMovement = GetComponent<MultiMovement>();
        animator = GetComponent<Animator>();
        photonView = GetComponent<PhotonView>();
        Transform legs = transform.Find("Legs");
        if (legs != null)
            legsSprite = legs.GetComponent<SpriteRenderer>();
    }

    [PunRPC]
    public void RPC_SpawnFakeBullet(Vector3 pos, Quaternion rot)
    {
        Instantiate(fakeBulletPrefab, pos, rot);
    }

    public void Shoot()
    {
        // Only local player can shoot
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        weapons.EquippedWeapon.Shoot();

    }

    public void Reload()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        weapons.EquippedWeapon.Reload();
    }

    private void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();

        if (multiMovement != null)
        {
            multiMovement.HandleMoveInput(input);
        }
        else
        {
            Debug.LogWarning($"{nameof(PlayerController)}: No PlayerMovement or MultiMovement found on this GameObject.");
        }
    }

    private void OnDodge(InputValue value)
    {
        if (!value.isPressed) return;

        if (multiMovement != null)
        {
            multiMovement.HandleDodgeInput();
        }
    }

    void Update()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (currentKnife != null) currentKnife.StartAttack();
            else
            {
                weapons.EquippedWeapon.StartShooting();
            }

        }

        if (Input.GetMouseButtonUp(0))
        {
            if (currentKnife != null) currentKnife.StopAttack();
            else weapons.EquippedWeapon.StopShooting();
        }
    }

    private void OnReload(InputValue value)
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        if (value.isPressed)
        {
            weapons.EquippedWeapon.Reload();
        }
    }

    private void OnSwitch1(InputValue value)
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        if (value.isPressed)
        {
            disableLeg = false;
            legsSprite.enabled = true;
            weapons.EquipGun(1);
            currentKnife = null;
        }
    }

    private void OnSwitch2(InputValue value)
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        if (value.isPressed)
        {
            disableLeg = false;
            legsSprite.enabled = true;
            weapons.EquipGun(2);
            currentKnife = null;
        }
    }

    private void OnSwitch3(InputValue value)
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        if (value.isPressed)
        {
            disableLeg = false;
            legsSprite.enabled = true;
            weapons.EquipGun(3);
            currentKnife = null;
        }
    }

    private void OnSwitch4(InputValue value)
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        if (value.isPressed)
        {
            disableLeg = true;
            legsSprite.enabled = false;
            weapons.EquipGun(4);
            animator.SetTrigger(equipKnifeTrigger);
            currentKnife = GetComponentInChildren<MPKnifeAttack>();
        }
    }
}
