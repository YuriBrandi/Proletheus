using UnityEngine;
using Unity.VisualScripting;

using UnityEditor;

public class Aircraft : MonoBehaviour
{
    [Header("Aircraft Settings")]
    public float speed = 20f;
    public float height = 300f;
    public float xRotation = 0f;
    public bool automaticHeightOffset = true;

    [Header("Debug Settings")]
    public bool drawGizmos = false;

    //public event Action<GameObject> OnFlyingTripEnd;
    
    private Vector3 startPoint, endPoint;
    private Vector3 direction;
    private bool isFlying = false;
    private int assignedLabel = -1;

    public void Start()
    {
        StartFlight();
    }

    void FixedUpdate()
    {
        if (isFlying && Vector3.Distance(transform.position, endPoint) < 50.0f)
        {
            isFlying = false;
            AircraftSpawner.TriggerFlyingTripEnd(gameObject);
        }
    }

    public void setCoords(Vector3 startPoint_, Vector3 endPoint_)
    {
        this.startPoint = startPoint_;
        this.endPoint = endPoint_;
    }

    private void StartFlight()
    {
        startPoint.y = endPoint.y = height;

        if (automaticHeightOffset)
        {   
            startPoint.y += Random.Range(-0.2f * height, 0.2f * height);
            endPoint.y += Random.Range(-0.2f * height, 0.2f * height);
        }

        Debug.Log("Aircraft pre position ");
        transform.position = startPoint;
        transform.LookAt(endPoint);

            
        Debug.Log("Aircraft post position ");

        direction = (endPoint - startPoint).normalized;

        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.linearVelocity = direction * speed;

        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.x = xRotation;
        transform.eulerAngles = currentRotation;

        isFlying = true;
    }

    public void setRadarLabel(int label)
    {
        if (label < -1 || label > 1)
        {
            Debug.LogError("Triying to assign invalid label");
            return;
        }

        this.assignedLabel = label;
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(startPoint, endPoint);

        string transLabel = "";
        switch (assignedLabel)
        {
            case -1: 
                transLabel = "Unassigned";
                Handles.color = Color.white;
                break;
            case 1:
                transLabel = "Enemy";
                Handles.color = Color.red;
                break;
            case 0:
                transLabel = "Friendly";
                Handles.color = Color.green;
                break;
        }

        Handles.Label(transform.position, $"Assigned Label: {transLabel}");

    }
}
