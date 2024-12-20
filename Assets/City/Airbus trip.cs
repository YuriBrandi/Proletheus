using UnityEngine;

public class PlaneFlyby : MonoBehaviour
{
    [Header("Plane Settings")]
    public Transform startPoint;   // Start position of the plane
    public Transform endPoint;     // End position of the plane
    public float speed = 20f;      // Speed of the plane

    [Header("Cycle Settings")]
    public float respawnDelay = 5f; // Time before the plane reappears

    [Header("Child Object Settings")]
    public Transform[] spinningChildren; // The child objects to spin (up to 2)
    public float spinSpeed = 100f;  // Rotation speed of the child objects

    private Vector3 start;
    private Vector3 end;
    private bool isFlying = false;

        /*
    void Start()
    {
        // Cache start and end positions
        start = startPoint.position;
        end = endPoint.position;

        // Initialize the plane's position
        transform.position = start;
        transform.LookAt(end); // Make the plane face the endpoint
        isFlying = true;
    }*/


    void Start()
{
    // Cache start and end positions
    start = startPoint.position;
    end = endPoint.position;

    // Initialize the plane's position
    transform.position = start;
    transform.LookAt(end); // Reorient the plane

    transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + 180, 0);

    isFlying = true;
}


    void Update()
    {
        if (isFlying)
        {
            // Move the plane
            transform.position = Vector3.MoveTowards(transform.position, end, speed * Time.deltaTime);

            // Check if the plane has reached the end point
            if (Vector3.Distance(transform.position, end) < 0.1f)
            {
                transform.position = start; // Reset the position to the start point
                transform.LookAt(end); // Reorient the plane
                isFlying = false;
                Invoke("RestartFlight", respawnDelay); // Restart flight after delay
            }
        }

        // Spin the child objects if assigned
        foreach (Transform child in spinningChildren)
        {
            if (child != null)
            {
                child.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
            }
        }
    }

    void RestartFlight()
    {
        isFlying = true;
    }
}
