using UnityEngine;
namespace TopDown.Movement
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Mover : MonoBehaviour
    {
        [SerializeField] private float movementSpeed;
        private Rigidbody2D body;
        protected Vector3 currentInput; //protected for child's class access
        public Vector3 CurrentInput => currentInput; // Chỉ cho phép xem {get;}

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            body.linearVelocity = movementSpeed * 80f * currentInput * Time.deltaTime; //if possible
        }
    }
}
