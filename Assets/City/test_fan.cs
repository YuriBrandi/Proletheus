using UnityEngine;

public class RotateObject : MonoBehaviour
{
    // Public variable for rotation speed, adjustable in the Inspector
    public float rotationSpeed = 100f;

    // Variable to keep track of the current rotation angle
    private float currentAngle = 0f;

    void Update()
    {
        // Increment the current angle based on rotation speed and time
        currentAngle += rotationSpeed * Time.deltaTime * -1f;

        if (TryGetComponent(out Renderer renderer))
        {
            transform.RotateAround(renderer.bounds.center, Vector3.forward, currentAngle);
        }
    }
}
