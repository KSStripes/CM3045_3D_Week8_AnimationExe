using UnityEngine;

namespace SciFiAnimation
{
    /// <summary>
    /// Smoothly follows a target with exponential interpolation.
    /// Maintains a constant offset and looks at the target with slight upward bias.
    /// </summary>
    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target; // Object to follow
        [SerializeField] private Vector3 offset = new Vector3(10f, 7f, -12f); // Camera position relative to target
        [SerializeField] private float followSharpness = 3.5f; // Smoothing factor (higher = faster response)
        [SerializeField] private float lookHeight = 1.2f; // Height offset for camera look direction

        /// <summary>Update the target to follow</summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>Smooth camera follow with exponential interpolation each frame</summary>
        private void LateUpdate()
        {
            if (target == null) return;

            // Calculate desired position based on target and offset
            Vector3 desired = target.position + offset;
            // Use exponential interpolation for frame-rate independent smoothing
            float blend = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, blend);

            // Rotate camera to look at target (slightly above for better view angle)
            Quaternion look = Quaternion.LookRotation(target.position + Vector3.up * lookHeight - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, blend);
        }
    }
}
