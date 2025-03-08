using UnityEngine;

public class ObjectFlyingTrip : MonoBehaviour
{
    [Header("Object Settings")]
    public float speed = 20f;      // Speed of the object
    public float height = 300f;     // Height adjustment for the object
    public float xRotation = 0f;
    
    [Header("City Corners")]
    public Transform A;   //Angle A
    public Transform B;   //Angle B
    public Transform C;   //Angle C
    public Transform D;   //Angle D

    [Header("Cycle Settings")]
    public float respawnDelay = 5f; // Time before the object reappears

    Vector3 startPoint, endPoint;
    private Rigidbody rb;
    private Vector3 direction;
    private bool isFlying = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) {
            Debug.LogError("Flying Object " + gameObject.name + " missing Rigidbody");
            return;
        }

        if (rb.useGravity) {
            Debug.LogWarning("Flying Object " + gameObject.name + " has gravity enabled, this may be undesired.");
        }

        GetRandomPoints(out startPoint, out endPoint);

        startPoint.y = endPoint.y = height;

        Debug.Log("Start trip for " + gameObject.name + $" Point: {startPoint}");
        Debug.Log("End trip for " + gameObject.name + $" Point: {endPoint}");

        // Initialize the object's position
        transform.position = startPoint;
        transform.LookAt(endPoint); // Reorient the object

        direction = (endPoint - startPoint).normalized;

        // Apply initial velocity
        rb.linearVelocity = direction * speed;

        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.x = xRotation; // Set the desired X rotation
        transform.eulerAngles = currentRotation; // Apply the new rotation

        isFlying = true;
    }

    void Update()
    {
        if (isFlying)
        {

            // Check if the object has reached the end point
            if (Vector3.Distance(transform.position, endPoint) < 0.1f)
            {
                transform.position = startPoint; // Reset the position to the start point
                transform.LookAt(endPoint); // Reorient the object

                isFlying = false;
                Invoke("RestartFlight", respawnDelay); // Restart flight after delay
            }
        }
    }

    private void GetRandomPoints(out Vector3 start, out Vector3 end)
    {
        // Define the sides of the rectangle
        Vector3[] side1 = { A.position, B.position };
        Vector3[] side2 = { B.position, C.position };
        Vector3[] side3 = { C.position, D.position };
        Vector3[] side4 = { D.position, A.position };

        // Store the sides in an array
        Vector3[][] sides = { side1, side2, side3, side4 };

        // Randomly choose a side
        int chosenSideIndex = Random.Range(0, sides.Length);
        Vector3[] chosenSide = sides[chosenSideIndex];

        // Get a random point on the chosen side
        start = GetRandomPointOnLine(chosenSide[0], chosenSide[1]);

        // Find the opposite side
        int oppositeSideIndex = (chosenSideIndex + 2) % sides.Length;
        Vector3[] oppositeSide = sides[oppositeSideIndex];

        // Get a random point on the opposite side
        end = GetRandomPointOnLine(oppositeSide[0], oppositeSide[1]);
    }

    private Vector3 GetRandomPointOnLine(Vector3 start, Vector3 end)
    {
        float t = Random.Range(0f, 1f); // Random value between 0 and 1
        return Vector3.Lerp(start, end, t); // Linear interpolation to get a point on the line
    }

    void RestartFlight()
    {
        isFlying = true;
    }

    void OnDrawGizmosSelected()
    {
        // Draw the path from start to end in the editor
        Gizmos.color = Color.green;
        Gizmos.DrawLine(startPoint, endPoint);
    }
}
