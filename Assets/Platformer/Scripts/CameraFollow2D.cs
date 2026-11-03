using UnityEngine;

namespace HOne.Platformer
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private LevelBounds2D worldBounds;
        [SerializeField, Range(0.05f, 0.45f)] private float horizontalAnchor = 0.24f;
        [SerializeField, Range(0.05f, 0.45f)] private float verticalAnchor = 0.25f;
        [SerializeField, Min(0.05f)] private float smoothTime = 0.42f;
        [SerializeField, Min(0.1f)] private float maxSpeed = 20f;

        private Camera followCamera;
        private Vector3 smoothVelocity;

        public void Configure(Transform followTarget, LevelBounds2D bounds)
        {
            target = followTarget;
            worldBounds = bounds;
        }

        private void Awake()
        {
            followCamera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (target == null || followCamera == null)
                return;

            var halfHeight = followCamera.orthographicSize;
            var halfWidth = halfHeight * followCamera.aspect;
            var desired = GetFocusPoint(halfWidth, halfHeight);
            desired.z = transform.position.z;

            if (worldBounds != null)
                desired = worldBounds.ClampCameraCenter(desired, halfWidth, halfHeight);

            var next = Vector3.SmoothDamp(
                transform.position,
                desired,
                ref smoothVelocity,
                smoothTime,
                maxSpeed,
                Time.deltaTime);

            if (worldBounds != null)
                next = worldBounds.ClampCameraCenter(next, halfWidth, halfHeight);
            next.z = transform.position.z;
            transform.position = next;
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
