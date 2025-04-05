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

    public Vector3 cityOrigin = Vector3.zero;

    [Header("Explosion Settings")]
    public float explosionDistanceThreshold = 5f;
    public float minimumExplosionDistance = 1.5f;

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
    private bool isExplosionEnabled;
    private bool hasDestroyedTarget = false;


    public void Initialize(Rigidbody enemyMissileRb)
    {
        this.enemyMissileRb = enemyMissileRb;

        agentRb = GetComponent<Rigidbody>();
        agentRb.linearVelocity = gameObject.transform.forward * missileSpeed;

        detectionDistance = previousDistance = Vector3.Distance(agentRb.position, enemyMissileRb.position);


        //Debug.Log("Missile agent initialized. Detection distance: " + detectionDistance);
    }

    public void Start()
    {
        if (cityOrigin == null)
        {
            Debug.LogError("City origin null");
            return;
        }
        if (Academy.Instance.IsCommunicatorOn)
        {
            // Check for curriculum parameter (will default to true if not present)
            isExplosionEnabled = Academy.Instance.EnvironmentParameters.GetWithDefault("require_explosion_signal", 1f) == 1f;
        }


    }

    public override void CollectObservations(VectorSensor sensor)
    {
        try
        {
            Vector3 relativePosition = enemyMissileRb.position - agentRb.position;
            Vector3 relativeVelocity = enemyMissileRb.linearVelocity - agentRb.linearVelocity;

            //print("relativePosition: " + relativePosition + " | relativeVelocity" +  relativeVelocity);

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
            Debug.Log("[OBSERVATION NULL EXCEPTION]");
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        try
        {
            float pitch = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
            float yaw = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
            int explodeSignal = actions.DiscreteActions[0];

            //Debug.Log("pitch: " + pitch + " | yaw: " + yaw + " | explode: " + explodeSignal);

            transform.Rotate(pitch * turnSpeed * Time.fixedDeltaTime, yaw * turnSpeed * Time.fixedDeltaTime, 0f, Space.Self);
            agentRb.linearVelocity = transform.forward * missileSpeed;

            float currentDistance = Vector3.Distance(agentRb.position, enemyMissileRb.position);
            float distanceDelta = previousDistance - currentDistance;
            AddReward(distanceDelta * 0.01f + (Vector3.Distance(enemyMissileRb.position, cityOrigin) * 0.01f));

            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, raycastDistance))
            {
                if (!isEnemyTarget(hit.collider.attachedRigidbody) && !hit.collider.CompareTag("targetMissile"))
                {
                    float collisionRiskPenalty = (raycastDistance - hit.distance) / raycastDistance;
                    AddReward(-0.01f * collisionRiskPenalty);
                }
            }

            if (currentDistance <= explosionDistanceThreshold)
            {
                AddReward(10.0f - (currentDistance / explosionDistanceThreshold));
                Debug.Log("[ExplosionEvent] Distance: " + currentDistance + " | reward: " + (1.0f - (currentDistance / explosionDistanceThreshold)));

                if (!isExplosionEnabled || explodeSignal == 1 || currentDistance <= minimumExplosionDistance)
                {
                    AddReward(30.0f);
                    OnEpisodeFinish();
                    Destroy(gameObject);
                }
            }

            if (currentDistance > Math.Pow(detectionDistance, 1.5))
            {
                Debug.Log("Target too far, ending episode. Current distance: " + currentDistance);
                AddReward(-1.0f);
                OnEpisodeFinish();
                Destroy(gameObject);
            }

            previousDistance = currentDistance;

            InterceptorBehaviour.OnMinDistanceDebug(currentDistance, explodeSignal);
        }
        catch (MissingReferenceException)
        {
            Debug.Log("[ACTION NULL EXCEPTION] hasDestroyedTarget: " + hasDestroyedTarget);

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
            Debug.Log("Collision with target missile.");
            AddReward(1.0f);
        }
        else if (!collision.gameObject.CompareTag("targetMissile"))
        {
            Debug.Log("Collision with non-target object.");
            AddReward(-1f);
        }

        OnEpisodeFinish();
        Destroy(gameObject);
    }

    private bool isEnemyTarget(Rigidbody rb)
    {
        return rb != null && rb == enemyMissileRb;
    }


    private void OnDrawGizmosSelected()
    {
        if (enemyMissileRb == null) return;

        // Draw a red line from this object to its target
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, enemyMissileRb.position);

        // Calculate distance
        float distance = Vector3.Distance(transform.position, enemyMissileRb.position);


        // Place label at midpoint of the line
        Vector3 midpoint = (transform.position + enemyMissileRb.position) / 2f;
        Handles.Label(midpoint, $"Distance: {distance:F2} units");


        // Draw a sphere at this object's position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 100f);

        // Draw a cube at the target's position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(enemyMissileRb.position, 100f);
    }
}
