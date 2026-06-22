using UnityEngine;

namespace SciFiAnimation
{
    /// <summary>
    /// Rotates the object continuously at a specified rate.
    /// Useful for spinning decorative elements or UI components.
    /// </summary>
    public sealed class Spin : MonoBehaviour
    {
        [SerializeField] private Vector3 degreesPerSecond = new Vector3(0f, 45f, 0f); // Rotation speed per axis (degrees/second)

        /// <summary>Update rotation speed at runtime</summary>
        public void Configure(Vector3 speed)
        {
            degreesPerSecond = speed;
        }

        /// <summary>Rotate object each frame based on configured speed</summary>
        private void Update()
        {
            transform.Rotate(degreesPerSecond * Time.deltaTime, Space.Self);
        }
    }
}
