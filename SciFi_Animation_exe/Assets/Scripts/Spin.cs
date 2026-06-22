using UnityEngine;

namespace SciFiAnimation
{
    public sealed class Spin : MonoBehaviour
    {
        [SerializeField] private Vector3 degreesPerSecond = new Vector3(0f, 45f, 0f);

        public void Configure(Vector3 speed)
        {
            degreesPerSecond = speed;
        }

        private void Update()
        {
            transform.Rotate(degreesPerSecond * Time.deltaTime, Space.Self);
        }
    }
}
