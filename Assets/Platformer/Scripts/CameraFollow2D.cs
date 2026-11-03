using UnityEngine;

namespace HOne.Platformer
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private LevelBounds2D worldBounds;
        [SerializeField] private Rigidbody2D targetBody;
        [SerializeField, Range(0.05f, 0.45f)] private float horizontalAnchor = 0.24f;
        [SerializeField, Range(0.05f, 0.45f)] private float verticalAnchor = 0.25f;
        [SerializeField, Min(0.01f)] private Vector2 deadZone = new Vector2(0.42f, 0.34f);
        [SerializeField, Min(0.1f)] private float maxCameraSpeed = 8f;
        [SerializeField, Min(0.1f)] private float maxCatchUpSpeed = 5.5f;
        [SerializeField, Min(0.1f)] private float positionGain = 3.5f;
        [SerializeField, Min(0.1f)] private float maxCameraAcceleration = 12f;

        private Camera followCamera;
        private Vector2 cameraVelocity;
        private bool tracking;

        public void Configure(Transform followTarget, LevelBounds2D bounds)
        {
            target = followTarget;
            worldBounds = bounds;
            targetBody = followTarget != null ? followTarget.GetComponent<Rigidbody2D>() : null;
        }

        private void Awake()
        {
            followCamera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            var deltaTime = Time.deltaTime;
            if (target == null || followCamera == null || deltaTime <= 0f)
                return;

            var halfHeight = followCamera.orthographicSize;
            var halfWidth = halfHeight * followCamera.aspect;
            var desiredCenter = GetFocusPoint(halfWidth, halfHeight);
            var targetVelocity = targetBody != null ? targetBody.linearVelocity : Vector2.zero;

            if (worldBounds != null)
            {
                var clampedCenter = worldBounds.ClampCameraCenter(desiredCenter, halfWidth, halfHeight);
                if (!Mathf.Approximately(clampedCenter.x, desiredCenter.x))
                    targetVelocity.x = 0f;
                if (!Mathf.Approximately(clampedCenter.y, desiredCenter.y))
                    targetVelocity.y = 0f;
                desiredCenter = clampedCenter;
            }

            var currentPosition = (Vector2)transform.position;
            var error = (Vector2)desiredCenter - currentPosition;
            var insideDeadZone = Mathf.Abs(error.x) <= deadZone.x && Mathf.Abs(error.y) <= deadZone.y;

            if (!tracking && !insideDeadZone)
                tracking = true;

            if (tracking && insideDeadZone && targetVelocity.sqrMagnitude <= 0.0025f && cameraVelocity.sqrMagnitude <= 0.0025f)
                tracking = false;

            var desiredVelocity = Vector2.zero;
            if (tracking)
            {
                var brakingAcceleration = GetBrakingAcceleration();
                desiredVelocity.x = GetDesiredAxisVelocity(error.x, targetVelocity.x, brakingAcceleration);
                desiredVelocity.y = GetDesiredAxisVelocity(error.y, targetVelocity.y, brakingAcceleration);
                desiredVelocity = Vector2.ClampMagnitude(desiredVelocity, maxCameraSpeed);
            }

            var axisAcceleration = maxCameraAcceleration / Mathf.Sqrt(2f);
            cameraVelocity.x = Mathf.MoveTowards(cameraVelocity.x, desiredVelocity.x, axisAcceleration * deltaTime);
            cameraVelocity.y = Mathf.MoveTowards(cameraVelocity.y, desiredVelocity.y, axisAcceleration * deltaTime);
            var nextPosition = currentPosition + cameraVelocity * deltaTime;
            var nextCenter = new Vector3(nextPosition.x, nextPosition.y, transform.position.z);

            if (worldBounds != null)
            {
                var clampedCenter = worldBounds.ClampCameraCenter(nextCenter, halfWidth, halfHeight);
                if (!Mathf.Approximately(clampedCenter.x, nextCenter.x))
                    cameraVelocity.x = 0f;
                if (!Mathf.Approximately(clampedCenter.y, nextCenter.y))
                    cameraVelocity.y = 0f;
                nextCenter = clampedCenter;
            }

            transform.position = nextCenter;
        }

        private float GetBrakingAcceleration()
        {
            return maxCameraAcceleration / Mathf.Sqrt(2f);
        }

        private float GetDesiredAxisVelocity(float error, float targetVelocity, float brakingAcceleration)
        {
            var distance = Mathf.Abs(error);
            var relativeSpeed = Mathf.Min(
                maxCatchUpSpeed,
                positionGain * distance,
                Mathf.Sqrt(2f * brakingAcceleration * distance));
            return targetVelocity + Mathf.Sign(error) * relativeSpeed;
        }

        private Vector3 GetFocusPoint(float halfWidth, float halfHeight)
        {
            return target.position + new Vector3(
                halfWidth * 2f * (0.5f - horizontalAnchor),
                halfHeight * 2f * (0.5f - verticalAnchor),
                0f);
        }
    }
}
