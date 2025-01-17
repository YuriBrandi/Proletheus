using UnityEngine;

public class MissileController : MonoBehaviour
{
    public Transform target; // Target point for the missile
    public float minSpeed = 10f; // Minimum speed
    public float maxSpeed = 20f; // Maximum speed
    public float minHeight = 5f; // Minimum height for the arc
    public float maxHeight = 15f; // Maximum height for the arc

    private Vector3 startPoint;
    private Vector3 controlPoint;
    private float speed;
    private float t = 0f;

    void Start()
    {
        // Initialize starting point
        startPoint = transform.position;

        // Randomize speed and height
        speed = Random.Range(minSpeed, maxSpeed);
        float height = Random.Range(minHeight, maxHeight);

        // Calculate control point for the arc
        Vector3 midPoint = (startPoint + target.position) / 2;
        controlPoint = midPoint + Vector3.up * height;
    }

    void Update()
    {
        // Increment progress based on speed and distance
        t += Time.deltaTime * speed / Vector3.Distance(startPoint, target.position);

        // Clamp t to 1 to avoid overshooting
        t = Mathf.Clamp01(t);

        // Calculate position using quadratic Bezier curve
        Vector3 position = Mathf.Pow(1 - t, 2) * startPoint
                           + 2 * (1 - t) * t * controlPoint
                           + Mathf.Pow(t, 2) * target.position;

        transform.position = position;

        // Rotate missile to face movement direction using tangent
        if (t < 1)
        {
            Vector3 tangent = 2 * (1 - t) * (controlPoint - startPoint) + 2 * t * (target.position - controlPoint);
            transform.forward = tangent.normalized;
        }
        else
        {
            enabled = false; // Stop updating once the target is reached
        }
    }

}
