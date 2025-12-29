using UnityEngine;
namespace TopDown.CameraControl
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float displacementMultiplier = 0.15f; //độ nhạy liếc

        [SerializeField] private Vector2 minBounds;
        [SerializeField] private Vector2 maxBounds;

        [Header("Pause Behavior")]
        [SerializeField] private bool followMouseWhilePaused = false;

        private float zPosition = -10;

        public void SetTarget(Transform t) => playerTransform = t; //Multi Player only

        private void Update()
        {
            if (playerTransform == null) return;

            if (!followMouseWhilePaused && UIManager.IsPaused || UIManager.IsCleared) return; // freeze camera

            //Tính vị trí con trỏ chuột từ màn hình ra thế giới 2D
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            //Khoảng cách giữa chuột và người chơi nhân với hệ số lệch
            Vector3 cameraDisplacement = (mousePosition - playerTransform.position) * displacementMultiplier;

            //Xác định vị trí cuối cùng
            Vector3 finalCameraPosition = playerTransform.position + cameraDisplacement;
            finalCameraPosition.z = zPosition;

            //Giới hạn vị trí Camera
            finalCameraPosition.x = Mathf.Clamp(finalCameraPosition.x, minBounds.x, maxBounds.x);
            finalCameraPosition.y = Mathf.Clamp(finalCameraPosition.y, minBounds.y, maxBounds.y);

            //Cập nhật vị trí mới cho Camera
            transform.position = finalCameraPosition;
        }
    }
}
