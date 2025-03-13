using System;
using UnityEngine;
using Unity.VisualScripting;

public class Aircraft : MonoBehaviour
{
    [Header("Aircraft Settings")]
    public float speed = 20f;
    public float height = 300f;
    public float xRotation = 0f;

    //public event Action<GameObject> OnFlyingTripEnd;
    
    private Vector3 startPoint, endPoint;
    private Vector3 direction;
    private bool isFlying = false;

    public void Start()
    {
        StartFlight();
    }

    void FixedUpdate()
    {
        if (isFlying && Vector3.Distance(transform.position, endPoint) < 50.0f)
        {
            isFlying = false;
            AircraftController.TriggerFlyingTripEnd(gameObject);
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(startPoint, endPoint);
    }
}
