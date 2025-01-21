using UnityEngine;

public class MissileController : MonoBehaviour
{
    public enum Direction
    {
        N, S, E, W, NE, NW, SE, SW, RANDOM
    }

    [Header("Missile Settings")]
    public GameObject missilePrefab;
    public float minSpeed = 10f; // Minimum speed
    public float maxSpeed = 20f; // Maximum speed
    public float minHeight = 5f; // Minimum height for the arc
    public float maxHeight = 15f; // Maximum height for the arc

    [Header("City Corners")]
    public Transform A;   //Angle A
    public Transform B;   //Angle B
    public Transform C;   //Angle C
    public Transform D;   //Angle D

    [Header("Trip Settings")] 
    public BoxCollider targetCollider;
    public Direction chosenDirection;
    public float offset = 200f; // Origin offset (makes missile appear from farther).

    private GameObject missile;
    private Vector3 startPoint;
    private Vector3 endPoint;
    private Vector3 controlPoint;
    private float speed;
    private float t = 0f;

    void Start()
    {
        if (missilePrefab == null)
        {
            Debug.LogError("Missile prefab is not assigned.");
            return;
        }

        // Instantiate the missile
        missile = Instantiate(missilePrefab);
        missile.name = "enemy_missile";

        Vector3 endPoint = GetRandomPointInBoxCollider(targetCollider);
        Debug.Log("Missile End Point (XZ): " + endPoint);

        // Initialize starting point
        startPoint = GetPointByDirection(chosenDirection, offset);
        Debug.Log($"Missile Starting Point ({chosenDirection}): {startPoint}");

        // Randomize speed and height
        speed = Random.Range(minSpeed, maxSpeed);
        float height = Random.Range(minHeight, maxHeight);

        // Apply to startPoint
        startPoint.y = height;

        // Calculate control point for the arc
        Vector3 midPoint = (startPoint + endPoint) / 2;
        controlPoint = midPoint + Vector3.up * height;
    }

    void Update()
    {
        if (missile == null)
        {
            return;
        }

        // Increment progress based on speed and distance
        t += Time.deltaTime * speed / Vector3.Distance(startPoint, endPoint);

        // Clamp t to 1 to avoid overshooting
        t = Mathf.Clamp01(t);

        // Calculate position using quadratic Bezier curve
        Vector3 position = Mathf.Pow(1 - t, 2) * startPoint
                           + 2 * (1 - t) * t * controlPoint
                           + Mathf.Pow(t, 2) * endPoint;

        missile.transform.position = position;

        // Rotate missile to face movement direction using tangent
        if (t < 1)
        {
            Vector3 tangent = 2 * (1 - t) * (controlPoint - startPoint) + 2 * t * (endPoint - controlPoint);
            missile.transform.forward = tangent.normalized;
        }
        else
        {
            enabled = false; // Stop updating once the target is reached
        }
    }
    

    Vector3 GetRandomPointInBoxCollider(BoxCollider collider)
    {
        // Get center and size of box collider
        Vector3 center = collider.bounds.center;
        Vector3 size = collider.bounds.size;

        // Get a random X and Z coordinate
        float randomX = Random.Range(center.x - size.x / 2, center.x + size.x / 2);
        float randomZ = Random.Range(center.z - size.z / 2, center.z + size.z / 2);

        // Get the coordinate with 
        return new Vector3(randomX, center.y, randomZ);
    }

    private Vector3 GetPointByDirection(Direction direction, float offset)
    {
        // Adjust based on direction
        if (direction == Direction.RANDOM)
        {
            direction = (Direction)Random.Range(0, 8); // Exclude "RANDOM" itself
        }

        Vector3 result = Vector3.zero;

        switch (direction)
        {
            case Direction.N:
                result = Vector3.Lerp(B.position, C.position, 0.5f) + Vector3.forward * offset;
                break;
            case Direction.S:
                result = Vector3.Lerp(A.position, D.position, 0.5f) + Vector3.back * offset;
                break;
            case Direction.E:
                result = Vector3.Lerp(C.position, D.position, 0.5f) + Vector3.right * offset;
                break;
            case Direction.W:
                result = Vector3.Lerp(A.position, B.position, 0.5f) + Vector3.left * offset;
                break;
            case Direction.NE:
                result = B.position + new Vector3(-offset, 0, offset);
                break;
            case Direction.NW:
                result = A.position + new Vector3(-offset, 0 -offset);
                break;
            case Direction.SE:
                result = C.position + new Vector3(offset, 0, offset);
                break;
            case Direction.SW:
                result = D.position + new Vector3(offset, 0, -offset);
                break;
        }

        return result;
    }

}
