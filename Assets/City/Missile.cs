using UnityEngine;

public class MissileCircularMotion : MonoBehaviour
{
    [Header("Missile Parameters")]
    [Tooltip("Speed at which the missile moves around the circle.")]
    public float angularSpeed = 30f; // Degrees per second

    [Tooltip("Radius of the circle in which the missile travels.")]
    public float radius = 10f;

    [Tooltip("Height at which the missile travels.")]
    public float height = 5f;

    [Header("Center of Circle")]
    [Tooltip("The center point of the circle around which the missile moves.")]
    public Vector3 centerPoint = new Vector3(0f, 0f, 0f);

    private float angle; // Current angle in degrees
    private Vector3 previousPosition; // Track previous position for direction calculation

    void Start()
    {
        // Initialize previous position to the starting position
        float radian = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(radian) * radius, height, Mathf.Sin(radian) * radius);
        previousPosition = centerPoint + offset;
        transform.position = previousPosition;
    }

    void Update()
    {
        // Update the angle based on angular speed
        angle += angularSpeed * Time.deltaTime;
        if (angle >= 360f) angle -= 360f;

        // Calculate the new position
        float radian = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(radian) * radius, height, Mathf.Sin(radian) * radius);
        Vector3 newPosition = centerPoint + offset;

        // Update the missile's position
        transform.position = newPosition;

        // Make the missile point in the direction of movement
        Vector3 direction = (newPosition - previousPosition).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        // Update the previous position for the next frame
        previousPosition = newPosition;
    }
}
