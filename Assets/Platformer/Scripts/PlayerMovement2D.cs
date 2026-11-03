using UnityEngine;
using UnityEngine.InputSystem;

namespace HOne.Platformer
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMovement2D : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float maxSpeed = 4.4f;
        [SerializeField, Min(0.1f)] private float acceleration = 6f;

        private Rigidbody2D body;
        private Vector2 moveInput;

        public Vector2 Velocity => body != null ? body.linearVelocity : Vector2.zero;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.linearDamping = 0f;
            body.angularDamping = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                moveInput = Vector2.zero;
                return;
            }

            moveInput = new Vector2(
                (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f),
                (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f));

            if (moveInput.sqrMagnitude > 1f)
                moveInput.Normalize();
        }

        private void FixedUpdate()
        {
            var desiredVelocity = moveInput * maxSpeed;
            body.linearVelocity = Vector2.MoveTowards(
                body.linearVelocity,
                desiredVelocity,
                acceleration * Time.fixedDeltaTime);
        }
    }
}
