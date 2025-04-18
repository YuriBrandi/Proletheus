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

    //public event Action<GameObject> OnFlyingTripEnd;

    [Header("Debug Settings")]
    private bool drawGizmos = false; // Made private for easier bulk change from AircraftSpawner
    
    private Vector3 startPoint, endPoint;
    private Vector3 direction;
    private bool isFlying = false;
    private int radarAssignedLabel = -1;

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

        transform.position = startPoint;
        transform.LookAt(endPoint);

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

        this.radarAssignedLabel = label;
    }

    public void enableGizmos(bool isEnabled)
    {
        drawGizmos = isEnabled;
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(startPoint, endPoint);

        string transLabel = "";
        switch (radarAssignedLabel)
        {
            case -1: 
                transLabel = "Unassigned";
                GUI.color = Color.white;
                break;
            case 1:
                transLabel = "Enemy";
                GUI.color = Color.red;
                break;
            case 0:
                transLabel = "Friendly";
                GUI.color = Color.green;
                break;
        }

        //Handles.Label(transform.position, $"Assigned Label: {transLabel}");

    }
}
