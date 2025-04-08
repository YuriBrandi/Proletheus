using System;
using Unity.MLAgents;
using UnityEngine;

using UnityEditor;

public class Missile : MonoBehaviour
{
    [Header("Trip Settings")]
    private Vector3 startPoint;
    private Vector3 endPoint;
    private Vector3 controlPoint;

    [Header("Debug Settings")]
    private bool drawGizmos = false;

    private int assignedLabel = -1;
    private Rigidbody rb;

    public void Initialize(Vector3 startPoint_, Vector3 endPoint_, float height_, bool drawGizmos_)
    {
        this.name = "enemy_missile_" + DateTime.Now.ToString("HHmmssfff");
        //this.name = "enemy_missile";

        // Get rb for launch
        rb = gameObject.GetComponent<Rigidbody>();

        // Check rb
        if (rb == null)
            Debug.LogError("Missing rigidbody on EnemyMissile.");

        startPoint = startPoint_;
        endPoint = endPoint_;

        // Apply to starting point
        startPoint.y = height_;

        // Compute control point for the parabolic trajectory (central point is raised)
        Vector3 midPoint = (startPoint + endPoint) / 2;
        controlPoint = midPoint + Vector3.up * height_;

        // Ensure position are non-NaN
        if (float.IsNaN(startPoint.x) || float.IsNaN(startPoint.y) || float.IsNaN(startPoint.z) ||
            float.IsNaN(endPoint.x) || float.IsNaN(endPoint.y) || float.IsNaN(endPoint.z) ||
            float.IsNaN(controlPoint.x) || float.IsNaN(controlPoint.y) || float.IsNaN(controlPoint.z))
        {
            Debug.LogError("Invalid position values detected.");
            return;
        }

        gameObject.layer = LayerMask.NameToLayer("Missiles");

        drawGizmos = drawGizmos_;

        LaunchMissile();
    }

    private void LaunchMissile()
    {
        // Compute directory
        Vector3 direction = (endPoint - startPoint).normalized;

        // Set missile to startpoint;
        transform.position = startPoint;

        // Get initial velociy for the parabolic launch
        float gravity = Physics.gravity.y;  // Use global gravity
        float distance = Vector3.Distance(startPoint, endPoint);
        float launchSpeed = Mathf.Sqrt(distance * -gravity / Mathf.Sin(2 * Mathf.Atan2(controlPoint.y - startPoint.y, distance)));

        // Set initial velocity
        Vector3 velocity = direction * launchSpeed;

        // Add force to rb
        rb.linearVelocity = velocity;

    }

    private void FixedUpdate()
    {

        if (transform.position.y < 0)
        {
            Debug.Log("[Missile]: start " + startPoint + " | end " + endPoint);
            Destroy(gameObject);
            return;
        }

        // Get current rb velocity
        if (rb.linearVelocity.sqrMagnitude > 0.1f) // Check if is moving
        {
            // Set direction
            Vector3 forwardDirection = rb.linearVelocity.normalized;

            // Allign missile rotation to movement direction
            Quaternion targetRotation = Quaternion.LookRotation(forwardDirection);

            // Apply Slerp with deltaTime for gradual rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
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
                GUI.color = Color.red;
                break;
            case 0:
                transLabel = "Friendly";
                Handles.color = Color.green;
                break;
        }

        Handles.Label(transform.position, $"Assigned Label: {transLabel}");

    }
}
