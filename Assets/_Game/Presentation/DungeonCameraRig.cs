using UnityEngine;

namespace AffixZero.Presentation
{
    public sealed class DungeonCameraRig : MonoBehaviour
    {
        private Transform target;
        private Camera view;
        public void Configure(Transform follow)
        {
            target = follow; view = GetComponent<Camera>();
            if (view != null) view.orthographicSize = 9f;
            Snap();
        }
        private void LateUpdate()
        {
            if (target == null || view == null) return;
            Vector3 desired = Clamped(target.position);
            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-7f * Time.unscaledDeltaTime));
        }
        private void Snap() { if (target != null && view != null) transform.position = Clamped(target.position); }
        private Vector3 Clamped(Vector3 point)
        {
            float halfHeight = view.orthographicSize;
            float halfWidth = halfHeight * Mathf.Max(1f, view.aspect);
            Rect bounds = DungeonWorld.Bounds;
            float x = Mathf.Clamp(point.x, bounds.xMin + halfWidth, bounds.xMax - halfWidth);
            float y = Mathf.Clamp(point.y, bounds.yMin + halfHeight, bounds.yMax - halfHeight);
            return new Vector3(x, y, -10);
        }
    }
}
