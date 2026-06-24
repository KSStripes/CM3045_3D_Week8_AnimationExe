using UnityEngine;

// Keeps the camera following the drone with a fixed offset and look height.
public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(10f, 7f, -12f);
    [SerializeField] private float followSpeed = 3.5f;
    [SerializeField] private float lookHeight = 1.2f;

    // Move the camera after the drone has moved for the frame.
    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * lookHeight);
    }
}
