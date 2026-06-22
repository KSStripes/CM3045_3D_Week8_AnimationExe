using UnityEngine;

namespace SciFiAnimation
{
    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(10f, 7f, -12f);
        [SerializeField] private float followSharpness = 3.5f;
        [SerializeField] private float lookHeight = 1.2f;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = target.position + offset;
            float blend = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, blend);
            Quaternion look = Quaternion.LookRotation(target.position + Vector3.up * lookHeight - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, blend);
        }
    }
}
