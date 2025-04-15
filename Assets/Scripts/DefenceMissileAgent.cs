using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System;

using UnityEditor;

public class DefenceMissileAgent : Agent
{
    [Header("Missile Settings")]
    public float turnSpeed = 500f;
    public float missileSpeed = 500f;

    [Header("Additional Settings")]
    public string targetMissileTag;
    public string aircraftTag;
    public Vector3 cityOrigin = Vector3.zero;

    [Header("Explosion Settings")]
    public float explosionDistanceThreshold = 5f;

    [Header("Raycast Settings")]
    public int raycastCount = 5;
    public float raycastAngle = 20f;
    public float raycastDistance = 50f;

    [Header("Debug Settings")]
    public bool drawGizmos = false;

    private Rigidbody agentRb;
    private Rigidbody enemyMissileRb;
    private float previousDistance;
    private float detectionDistance;
    private bool hasDestroyedTarget = false;
    private int radarAssignedLabel = -1;


    public void Initialize(Rigidbody enemyMissileRb)
    {
        this.enemyMissileRb = enemyMissileRb;

        agentRb = GetComponent<Rigidbody>();
        agentRb.linearVelocity = gameObject.transform.forward * missileSpeed;

        detectionDistance = previousDistance = Vector3.Distance(agentRb.position, enemyMissileRb.position);
    }

    public void Start()
    {
        if (cityOrigin == null)
        {
            Debug.LogError("City origin null");
            return;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        try
        {
            Vector3 relativePosition = enemyMissileRb.position - agentRb.position;
            Vector3 relativeVelocity = enemyMissileRb.linearVelocity - agentRb.linearVelocity;

            sensor.AddObservation(relativePosition.normalized);
            sensor.AddObservation(relativeVelocity.normalized);
            sensor.AddObservation(agentRb.linearVelocity.normalized);
            sensor.AddObservation(relativePosition.magnitude / 100f);

            float angleStep = raycastAngle / (raycastCount - 1);
            float startAngle = -raycastAngle / 2;

            for (int i = 0; i < raycastCount; i++)
            {
                Quaternion rotation = Quaternion.Euler(0, startAngle + angleStep * i, 0);
                Vector3 rayDirection = rotation * transform.forward;

                Debug.DrawRay(transform.position, rayDirection.normalized * raycastDistance, Color.magenta);
                if (Physics.Raycast(transform.position, rayDirection, out RaycastHit hit, raycastDistance))
                {
                    sensor.AddObservation(isEnemyTarget(hit.collider.attachedRigidbody) ? 1f : -1f);
                }
                else
                {
                    sensor.AddObservation(0f);
                }
            }
        }
        catch (MissingReferenceException)
        {
            Debug.Log("Target Missile reached during observation.");
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        try
        {
            float pitch = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
            float yaw = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

            transform.Rotate(pitch * turnSpeed * Time.fixedDeltaTime, yaw * turnSpeed * Time.fixedDeltaTime, 0f, Space.Self);
            agentRb.linearVelocity = transform.forward * missileSpeed;

            float currentDistance = Vector3.Distance(agentRb.position, enemyMissileRb.position);
            float distanceDelta = previousDistance - currentDistance;
            AddReward(distanceDelta * 0.001f + (Vector3.Distance(enemyMissileRb.position, cityOrigin) * 0.00001f));

            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, raycastDistance))
            {
                if (!isEnemyTarget(hit.collider.attachedRigidbody) && !hit.collider.CompareTag("targetMissile"))
                {
                    float collisionRiskPenalty = (raycastDistance - hit.distance) / raycastDistance;
                    AddReward(-2f * collisionRiskPenalty);
                }
            }

            if (currentDistance <= explosionDistanceThreshold)
            {
                AddReward(50.0f);
                OnEpisodeFinish();
                CurriculumDebug.OnEnemyMissileDestroyed(true);
                Destroy(enemyMissileRb.gameObject); //TODO: gestire esplosione
                Destroy(gameObject);
            }

            if (currentDistance > Math.Pow(detectionDistance, 1.5))
            {
                Debug.Log("Target too far, ending episode. Current distance: " + currentDistance);
                AddReward(-20.0f);
                OnEpisodeFinish();
                Destroy(gameObject);
            }

            previousDistance = currentDistance;

        }
        catch (MissingReferenceException)
        {
            Debug.Log("[ACTION NULL EXCEPTION] hasDestroyedTarget: " + hasDestroyedTarget);

            // Too low rewards may confuse the agent at the beginning of the training (this will be happening a lot)
            if (!hasDestroyedTarget)
                AddReward(-1.0f); 


            OnEpisodeFinish();
            Destroy(gameObject);
        }
    }

    private void OnEpisodeFinish()
    {
        CurriculumDebug.OnEpisodeFinish(GetCumulativeReward());
        EndEpisode();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isEnemyTarget(collision.rigidbody))
        {
            hasDestroyedTarget = true;
            Debug.Log("DefenceMissile collision with target missile.");
            AddReward(50.0f);
            CurriculumDebug.OnEnemyMissileDestroyed(true);
        }
        else if (!collision.gameObject.CompareTag(targetMissileTag))
        {
            Debug.Log("DefenceMissile collision with non-target object: " + collision.gameObject.name + " on " + collision.transform.position);
            AddReward(-30f);

            if(collision.gameObject.CompareTag(aircraftTag))
                collision.gameObject.SetActive(false);
        }

        OnEpisodeFinish();
        Destroy(gameObject);
    }

    private bool isEnemyTarget(Rigidbody rb)
    {
        return rb != null && rb == enemyMissileRb;
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


    private void OnDrawGizmos()
    {
        if (!drawGizmos || enemyMissileRb == null) return;

        // Draw a red line from this object to its target
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, enemyMissileRb.position);

        // Calculate distance
        float distance = Vector3.Distance(transform.position, enemyMissileRb.position);


        // Place label at midpoint of the line
        Vector3 midpoint = (transform.position + enemyMissileRb.position) / 2f;
        Handles.Label(midpoint, $"Distance: {distance:F2} units");

        // Label for classification
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

        Handles.Label(transform.position, $"Assigned Label: {transLabel}");


        // Draw a sphere at this object's position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 50f);

        // Draw a cube at the target's position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(enemyMissileRb.position, explosionDistanceThreshold);
    }
}
