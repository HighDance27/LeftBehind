using UnityEngine;
using UnityEngine.InputSystem;
namespace TopDown.Movement
{
    public class PlayerRotation : Rotator
    {
        [Header("Torso & Legs")]
        [SerializeField] private Transform torso;
        [SerializeField] private Transform legs;

        [Header("Mover References")]
        [SerializeField] private Mover playerMover;

        public Quaternion LastLegsRotation { get; private set; }

        private void OnLook(InputValue value)
        {
            if (UIManager.IsPaused || UIManager.IsCleared) return;

            //Chuyển đổi tọa độ chuột từ màn hình (Screen Space) sang thế giới Game (World Space)
            //value.Get<Vector2>() lấy vị trí chuột hiện tại (pixel trên màn hình)
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(value.Get<Vector2>());
            LookAt(torso, mousePosition);
        }

        private void Update()
        {
            if (UIManager.IsPaused) return;
            //Xoay chân để hướng theo hướng di chuyển
            Vector3 legsLookPoint = transform.position + new Vector3(playerMover.CurrentInput.x, playerMover.CurrentInput.y);
            LookAt(legs, legsLookPoint);

            //Lưu giá trị xoay hiện tại, để giữ cho chân không bị xoay về mặc định
            //khi người chơi thả phím di chuyển
            LastLegsRotation = legs.rotation;
        }
    }
}
