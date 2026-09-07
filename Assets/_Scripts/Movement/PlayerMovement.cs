using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace TopDown.Movement
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerMovement : Mover
    {
        private bool canMove = true;
        private float originalMovementSpeed;

        [Header("Dodge Settings")]
        [SerializeField] private float dodgeDistance = 3f;
        [SerializeField] private float dodgeDuration = 0.2f;
        [SerializeField] private Vector3 dodgeScale = new Vector3(0.2f, 0.2f, 1f);
        private Coroutine dodgeCoroutine;

        [Header("VFX")]
        [SerializeField] private GameObject dodgeDustPrefab;

        [Header("Collision During Dodge")]
        [SerializeField] private string dodgeLayer = "PlayerDodge";
        [SerializeField] private LayerMask wallLayers;
        [SerializeField] private float wallSkin = 0.02f; // pull back from hit
        [SerializeField] private float dodgeCooldown = 1f;
        private bool canDodge = true;


        private Collider2D col;

        private void Start()
        {
            col = GetComponent<Collider2D>();
            originalMovementSpeed = movementSpeed;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Tree"))
            {
                movementSpeed = originalMovementSpeed * 0.5f;
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Tree"))
            {
                movementSpeed = originalMovementSpeed * 0.5f;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Tree"))
            {
                movementSpeed = originalMovementSpeed;
            }
        }

        public void HandleMoveInput(Vector2 input)
        {
            if (!canMove)
            {
                currentInput = Vector3.zero; //Dừng di chuyển
                return;
            }

            // Chuyển đổi Vector2 từ InputSystem sang Vector3 để tương thích với hệ thống di chuyển 2D/3D.
            Vector3 playerInput = new Vector3(input.x, input.y, 0);
            currentInput = playerInput;
        }

        public void StopMovement()
        {
            canMove = false;
            currentInput = Vector3.zero; //Reset hướng di chuyển để đứng yên

        }

        public void HandleDodgeInput()
        {
            if (!canDodge) return;

            if (dodgeCoroutine != null)
                StopCoroutine(dodgeCoroutine); //anti spam

            dodgeCoroutine = StartCoroutine(DodgeRoutine());
        }


        private IEnumerator DodgeRoutine()
        {
            canMove = false; // lock normal movement
            canDodge = false;

            Transform torso = transform.Find("Weapons");
            Transform legs = transform.Find("Legs");

            Vector3 originalScale = new Vector3(1, 1, 1);
            Vector3 startPos = transform.position;
            Vector3 direction = currentInput.normalized;

            if (direction == Vector3.zero)
                direction = Vector3.right; //mặc định né sang phải

            if (dodgeDustPrefab)
            {
                Quaternion rot = Quaternion.LookRotation(Vector3.forward, -direction); //leave the vfx at the back of Player
                Instantiate(dodgeDustPrefab, transform.position, rot);
            }

            //Đổi layer sang DodgeLayer để tránh va chạm với layer LowObstacle
            int prevLayer = gameObject.layer;
            gameObject.layer = LayerMask.NameToLayer(dodgeLayer);

            //giới hạn khoảng cách khi đụng tường
            float allowed = dodgeDistance;
            if (col != null)
            {
                var filter = new ContactFilter2D { useLayerMask = true, layerMask = wallLayers, useTriggers = false };
                RaycastHit2D[] hits = new RaycastHit2D[1]; //lấy tia đầu tiên của Ray
                //dùng collider của Player, quét theo hướng của dodgeDistance, trả về những colliders mà nó bắn tới
                int hitCount = col.Cast(direction, filter, hits, dodgeDistance);
                if (hitCount > 0)
                {
                    //để lại 1 khoảng trống để tránh kẹt
                    allowed = Mathf.Max(0f, hits[0].distance - wallSkin);
                    //eg: distance=1 => allowed = 1-0.02
                }
            }

            Vector3 targetPos = startPos + direction * allowed;
            float t = 0;

            while (t < 1f)
            {
                t += Time.deltaTime / dodgeDuration;
                transform.position = Vector3.Lerp(startPos, targetPos, t);

                //phóng to sprite
                if (torso) torso.localScale = Vector3.Lerp(originalScale, dodgeScale, t);
                if (legs) legs.localScale = Vector3.Lerp(originalScale, dodgeScale, t);
                yield return null;  //tạm dừng Coroutine và chờ frame tiếp theo mới chạy tiếp, tránh teleport
            }
            //trở lại scale gốc
            if (torso) torso.localScale = originalScale;
            if (legs) legs.localScale = originalScale;

            gameObject.layer = prevLayer;
            canMove = true; // re-enable movement
            yield return new WaitForSeconds(dodgeCooldown);
            canDodge = true;
        }

    }
}
