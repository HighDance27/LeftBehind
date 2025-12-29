using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TopDown.Movement
{
    [RequireComponent(typeof(PlayerInput))]
    public class MultiMovement : Mover
    {
        private bool canMove = true;

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
        private PhotonView view;

        private void Start()
        {
            col = GetComponent<Collider2D>();
            view = GetComponent<PhotonView>();
        }

        public void HandleMoveInput(Vector2 input)
        {
            if (view.IsMine)
            {
                if (!canMove)
                {
                    currentInput = Vector3.zero;
                    return;
                }

                // Chuyển đổi Vector2 từ InputSystem sang Vector3 để tương thích với hệ thống di chuyển 2D/3D.
                Vector3 playerInput = new Vector3(input.x, input.y, 0);
                currentInput = playerInput;
            }
        }

        public void StopMovement()
        {
            canMove = false;
            currentInput = Vector3.zero;
        }

        public void HandleDodgeInput()
        {
            if (!view.IsMine) return; //chỉ player local mới xử lý
            if (!canDodge) return;

            // Decide dodge direction ONCE on the owner
            Vector3 direction = currentInput.sqrMagnitude > 0.001f
                ? currentInput.normalized
                : transform.right; //mặc định né qua phải

            //yêu cầu mọi người hiển thị hoạt ảnh trên máy họ
            view.RPC("RPC_DoDodge", RpcTarget.All, direction);
        }

        [PunRPC]
        private void RPC_DoDodge(Vector3 direction)
        {
            if (dodgeCoroutine != null)
                StopCoroutine(dodgeCoroutine);

            dodgeCoroutine = StartCoroutine(DodgeRoutine(direction));
        }

        private IEnumerator DodgeRoutine(Vector3 direction)
        {
            canMove = false;
            canDodge = false;

            Transform torso = transform.Find("Weapons");
            Transform legs = transform.Find("Legs");

            Vector3 originalScale = new Vector3(0.15f, 0.15f, 0.1f);
            Vector3 startPos = transform.position;

            if (direction == Vector3.zero)
                direction = transform.right;

            if (dodgeDustPrefab)
            {
                Quaternion rot = Quaternion.LookRotation(Vector3.forward, -direction);
                Instantiate(dodgeDustPrefab, transform.position, rot);
            }

            int prevLayer = gameObject.layer;
            gameObject.layer = LayerMask.NameToLayer(dodgeLayer);

            float allowed = dodgeDistance;
            if (col != null)
            {
                var filter = new ContactFilter2D { useLayerMask = true, layerMask = wallLayers, useTriggers = false };
                RaycastHit2D[] hits = new RaycastHit2D[1]; //khai báo mảng chỉ cần 1 kết quả

                //lấy colldier của Player (circle) và đẩy nó đi khoảng dodgeDistance
                int hitCount = col.Cast(direction, filter, hits, dodgeDistance);
                if (hitCount > 0)
                {
                    allowed = Mathf.Max(0f, hits[0].distance - wallSkin);
                }
            }

            Vector3 targetPos = startPos + direction * allowed;
            float t = 0;

            while (t < 1f)
            {
                t += Time.deltaTime / dodgeDuration;
                transform.position = Vector3.Lerp(startPos, targetPos, t);

                if (torso) torso.localScale = Vector3.Lerp(originalScale, dodgeScale, t);
                if (legs) legs.localScale = Vector3.Lerp(originalScale, dodgeScale, t);

                yield return null;
            }

            if (torso) torso.localScale = originalScale;
            if (legs) legs.localScale = originalScale;

            gameObject.layer = prevLayer;
            canMove = true;
            yield return new WaitForSeconds(dodgeCooldown);
            canDodge = true;
        }
    }
}
