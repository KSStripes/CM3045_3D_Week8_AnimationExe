using UnityEngine;

// Spins an object at a constant rate.
public class Spin : MonoBehaviour
{
    public Vector3 degreesPerSecond = new Vector3(0f, 45f, 0f);

    // Rotate a little every frame.
    private void Update()
    {
        transform.Rotate(degreesPerSecond * Time.deltaTime);
    }
}
