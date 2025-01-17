using UnityEngine;

public class ObjectFlyingTrip : MonoBehaviour
{
    [Header("Object Settings")]
    public Transform startPoint;   // Start position of the object
    public Transform endPoint;     // End position of the object
    public float speed = 20f;      // Speed of the object
    public float xRotation = 0f;
    
    [Header("Cycle Settings")]
    public float respawnDelay = 5f; // Time before the object reappears
    public float height = 300f;     // Height adjustment for the object



    private Vector3 start;
    private Vector3 end;
    private bool isFlying = false;

    void Start()
    {
        // Cache start and end positions with height adjustment
        start = startPoint.position + new Vector3(0, height, 0);
        end = endPoint.position + new Vector3(0, height, 0);

        // Initialize the object's position
        transform.position = start;
        transform.LookAt(end); // Reorient the object

        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.x = xRotation; // Set the desired X rotation
        transform.eulerAngles = currentRotation; // Apply the new rotation

        isFlying = true;
    }

    void Update()
    {
        if (isFlying)
        {
            // Move the object
            transform.position = Vector3.MoveTowards(transform.position, end, speed * Time.deltaTime);

            // Check if the object has reached the end point
            if (Vector3.Distance(transform.position, end) < 0.1f)
            {
                transform.position = start; // Reset the position to the start point
                transform.LookAt(end); // Reorient the object

                //isFlying = false;
                Invoke("RestartFlight", respawnDelay); // Restart flight after delay
            }
        }
    }

    void RestartFlight()
    {
        isFlying = true;
    }
}
