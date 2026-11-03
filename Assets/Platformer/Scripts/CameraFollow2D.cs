using UnityEngine;

namespace HOne.Platformer
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private LevelBounds2D worldBounds;
        [SerializeField] private Rigidbody2D targetBody;
        [SerializeField] private PlayerMovement2D targetMovement;
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
            targetMovement = followTarget != null ? followTarget.GetComponent<PlayerMovement2D>() : null;
        }

        private void Awake()
        {
            followCamera = GetComponent<Camera>();
            if (target != null)
            {
                if (targetBody == null)
                    targetBody = target.GetComponent<Rigidbody2D>();
                if (targetMovement == null)
                    targetMovement = target.GetComponent<PlayerMovement2D>();
            }
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
            var maximumTargetVelocity = targetMovement != null
                ? new Vector2(targetMovement.MaxSpeed, 0f)
                : Vector2.zero;

            if (worldBounds != null)
            {
                var clampedCenter = worldBounds.ClampCameraCenter(desiredCenter, halfWidth, halfHeight);
                if (!Mathf.Approximately(clampedCenter.x, desiredCenter.x))
                {
                    targetVelocity.x = 0f;
                    maximumTargetVelocity.x = 0f;
                }
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
            var axisSpeedLimit = maxCameraSpeed / Mathf.Sqrt(2f);
            var cameraAcceleration = new Vector2(
                Mathf.Clamp((desiredVelocity.x - cameraVelocity.x) / deltaTime, -axisAcceleration, axisAcceleration),
                Mathf.Clamp((desiredVelocity.y - cameraVelocity.y) / deltaTime, -axisAcceleration, axisAcceleration));

            if (tracking)
            {
                cameraAcceleration.x = GetSafeAxisAcceleration(error.x, targetVelocity.x, cameraVelocity.x,
                    maximumTargetVelocity.x, cameraAcceleration.x, axisAcceleration, deltaTime, axisSpeedLimit);
                cameraAcceleration.y = GetSafeAxisAcceleration(error.y, targetVelocity.y, cameraVelocity.y,
                    maximumTargetVelocity.y, cameraAcceleration.y, axisAcceleration, deltaTime, axisSpeedLimit);
            }

            cameraAcceleration.x = LimitAxisAcceleration(cameraVelocity.x, cameraAcceleration.x,
                deltaTime, axisSpeedLimit);
            cameraAcceleration.y = LimitAxisAcceleration(cameraVelocity.y, cameraAcceleration.y,
                deltaTime, axisSpeedLimit);
            cameraVelocity += cameraAcceleration * deltaTime;
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

        private float GetSafeAxisAcceleration(float error, float targetVelocity, float cameraAxisVelocity,
            float maximumTargetSpeed, float requestedAcceleration, float axisAcceleration, float deltaTime,
            float axisSpeedLimit)
        {
            var direction = Mathf.Sign(error);
            if (direction == 0f)
                direction = Mathf.Sign(cameraAxisVelocity);
            if (direction == 0f)
                direction = Mathf.Sign(targetVelocity);
            if (direction == 0f)
                return requestedAcceleration;

            var distance = Mathf.Abs(error);
            var cameraVelocityTowardTarget = direction * cameraAxisVelocity;
            var requestedVelocityTowardTarget = cameraVelocityTowardTarget
                + direction * requestedAcceleration * deltaTime;
            var minimumReachableVelocity = Mathf.Max(-axisSpeedLimit,
                cameraVelocityTowardTarget - axisAcceleration * deltaTime);
            var maximumReachableVelocity = Mathf.Min(axisSpeedLimit,
                cameraVelocityTowardTarget + axisAcceleration * deltaTime);

            // After this frame, the remaining distance must still cover maximum-speed target motion
            // during one frame plus the camera's stopping distance under maximum braking.
            var maximumSafeClosingSpeed = Mathf.Sqrt(
                axisAcceleration * axisAcceleration * deltaTime * deltaTime
                + 2f * axisAcceleration * distance) - axisAcceleration * deltaTime;
            var maximumSafeCameraVelocity = maximumSafeClosingSpeed - maximumTargetSpeed;
            var safeMaximumReachableVelocity = Mathf.Min(maximumReachableVelocity, maximumSafeCameraVelocity);
            var nextVelocityTowardTarget = safeMaximumReachableVelocity < minimumReachableVelocity
                ? minimumReachableVelocity
                : Mathf.Clamp(requestedVelocityTowardTarget, minimumReachableVelocity, safeMaximumReachableVelocity);
            var nextCameraVelocity = direction * nextVelocityTowardTarget;
            return (nextCameraVelocity - cameraAxisVelocity) / deltaTime;
        }

        private static float LimitAxisAcceleration(float currentVelocity, float acceleration,
            float deltaTime, float maximumSpeed)
        {
            var nextVelocity = Mathf.Clamp(currentVelocity + acceleration * deltaTime, -maximumSpeed, maximumSpeed);
            return (nextVelocity - currentVelocity) / deltaTime;
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
