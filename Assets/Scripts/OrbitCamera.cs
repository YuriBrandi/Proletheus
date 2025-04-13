using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // The object to orbit around

    [Header("Orbit Settings")]
    public float distance = 10f; // Distance from the target
    public float sensitivity = 100f; // Mouse sensitivity

    [Header("Zoom Settings")]
    public float zoomSensitivity = 2f; // Mouse wheel zoom sensitivity
    public float minDistance = 5f; // Minimum zoom distance
    public float maxDistance = 20f; // Maximum zoom distance

    [Header("Clamping Settings")]
    public float minYAngle = 0f; // Minimum vertical angle (prevent going below ground level)
    public float maxYAngle = 80f; // Maximum vertical angle

    private float currentX = 0f; // Current horizontal angle
    private float currentY = 20f; // Current vertical angle (start slightly above ground level)

    private Vector2 lastMousePosition; // To store the last mouse position for infinite movement

    void Update()
    {
        // Infinite mouse movement
        if (Input.GetMouseButton(0)) // Check if the left mouse button is held down
        {
            Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            // Adjust angles based on mouse movement
            currentX += mouseDelta.x * sensitivity * Time.deltaTime;
            currentY -= mouseDelta.y * sensitivity * Time.deltaTime;

            // Clamp the vertical angle to prevent going below ground level
            currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);
        }

        // Handle zoom with the mouse wheel
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        distance -= scrollInput * zoomSensitivity;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Wrap the horizontal angle for infinite rotation
        currentX = Mathf.Repeat(currentX, 360f);
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("No target assigned for OrbitCamera.");
            return;
        }

        // Calculate the new camera position
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);

        // Update the camera position and look at the target
        transform.position = target.position + offset;
        transform.LookAt(target);
    }

    public float getCameraDistance()
    {
        return distance;
    }

    public Transform getTarget()
    {
        return target;
    }
}