using UnityEngine;

public class ObjectFlyingTrip : MonoBehaviour
{
    [Header("Object Settings")]
    public float speed = 20f;
    public float height = 300f;
    public float xRotation = 0f;

    [Header("City Corners")]
    public Transform A;
    public Transform B;
    public Transform C;
    public Transform D;

    [Header("Cycle Settings")]
    public float respawnDelay = 5f;

    private Vector3 startPoint, endPoint;
    private Rigidbody rb;
    private Vector3 direction;
    private bool isFlying = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Flying Object " + gameObject.name + " missing Rigidbody");
            return;
        }

        if (rb.useGravity)
        {
            Debug.LogWarning("Flying Object " + gameObject.name + " has gravity enabled, this may be undesired.");
        }

        StartNewFlight();
    }

    void FixedUpdate()
    {
        if (isFlying && Vector3.Distance(transform.position, endPoint) < 50.0f)
        {
            isFlying = false;
            Invoke("StartNewFlight", respawnDelay);
        }
    }

    private void StartNewFlight()
    {
        GetRandomPoints(out startPoint, out endPoint);
        startPoint.y = endPoint.y = height;

        transform.position = startPoint;
        transform.LookAt(endPoint);

        direction = (endPoint - startPoint).normalized;
        rb.linearVelocity = direction * speed;

        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.x = xRotation;
        transform.eulerAngles = currentRotation;

        isFlying = true;
    }

    private void GetRandomPoints(out Vector3 start, out Vector3 end)
    {
        Vector3[] side1 = { A.position, B.position };
        Vector3[] side2 = { B.position, C.position };
        Vector3[] side3 = { C.position, D.position };
        Vector3[] side4 = { D.position, A.position };
        Vector3[][] sides = { side1, side2, side3, side4 };

        int chosenSideIndex = Random.Range(0, sides.Length);
        Vector3[] chosenSide = sides[chosenSideIndex];
        start = GetRandomPointOnLine(chosenSide[0], chosenSide[1]);

        int oppositeSideIndex = (chosenSideIndex + 2) % sides.Length;
        Vector3[] oppositeSide = sides[oppositeSideIndex];
        end = GetRandomPointOnLine(oppositeSide[0], oppositeSide[1]);
    }

    private Vector3 GetRandomPointOnLine(Vector3 start, Vector3 end)
    {
        float t = Random.Range(0f, 1f);
        return Vector3.Lerp(start, end, t);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(startPoint, endPoint);
    }
}
