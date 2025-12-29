using Photon.Pun;
using TopDown.Movement;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiRotation : Rotator
{
    [Header("Torso & Legs")]
    [SerializeField] private Transform torso;
    [SerializeField] private Transform legs;

    [Header("Mover References")]
    [SerializeField] private Mover playerMover;

    public Quaternion LastLegsRotation { get; private set; }
    private PhotonView view;

    private void Start()
    {
        if (view == null)
            view = GetComponent<PhotonView>();
    }

    public void OnLook(InputValue value)
    {
        if (view == null) return;
        if (!view.IsMine) return;
        if (view.IsMine)
        {
            if (UIManager.IsPaused || UIManager.IsCleared) return;
            //World position
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(value.Get<Vector2>());
            LookAt(torso, mousePosition);
        }
    }

    private void Update()
    {
        if (view.IsMine)
        {
            if (UIManager.IsPaused) return;
            //xoay chân để hướng theo hướng đang đi
            Vector3 legsLookPoint = transform.position + new Vector3(playerMover.CurrentInput.x, playerMover.CurrentInput.y);
            LookAt(legs, legsLookPoint);

            //giữ lại hướng chân khi dừng di chuyển
            LastLegsRotation = legs.rotation;
        }
    }
}
