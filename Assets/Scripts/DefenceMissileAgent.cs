using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System;

public class DefenceMissileAgent : Agent
{
    [Header("Missile Settings")]
    public float turnSpeed = 500f;
    public float missileSpeed = 500f;

    [Header("Reward Settings")]
    public float explosionDistanceThreshold = 5f;
    public float minimumExplosionDistance = 1.5f;

    [Header("Raycast Settings (raycastCount da calcolare nelle observations)")]
    public int raycastCount = 5;
    public float raycastAngle = 20f;
    public float raycastDistance = 50f;

    private Rigidbody agentRb;
    private Rigidbody enemyMissileRb;
    private float previousDistance;
    private float detectionDistance;
    private bool hasDestroyedTarget = false;

    public void Initialize(Rigidbody enemyMissileRb)
    {
        this.enemyMissileRb = enemyMissileRb;
        
        agentRb = GetComponent<Rigidbody>();
        agentRb.linearVelocity = gameObject.transform.forward * missileSpeed;

        detectionDistance = previousDistance = Vector3.Distance(agentRb.position, enemyMissileRb.position);


        //Debug.Log("Missile agent initialized. Detection distance: " + detectionDistance);
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
            float explodeSignal = actions.ContinuousActions[2];

            //Debug.Log("pitch: " + pitch + " | yaw: " + yaw + " | explode: " + explodeSignal);

            transform.Rotate(pitch * turnSpeed * Time.fixedDeltaTime, yaw * turnSpeed * Time.fixedDeltaTime, 0f, Space.Self);
            agentRb.linearVelocity = transform.forward * missileSpeed;

            float currentDistance = Vector3.Distance(agentRb.position, enemyMissileRb.position);
            float distanceDelta = previousDistance - currentDistance;
            AddReward(distanceDelta * 0.01f);

            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, raycastDistance))
            {
                if (!isEnemyTarget(hit.collider.attachedRigidbody) && !hit.collider.CompareTag("targetMissile"))
                {
                    float collisionRiskPenalty = (raycastDistance - hit.distance) / raycastDistance;
                    AddReward(-0.01f * collisionRiskPenalty);
                }
            }

            if (currentDistance <= minimumExplosionDistance ||
                (currentDistance <= explosionDistanceThreshold && explodeSignal > 0.5f))
            {
                Debug.Log("Explosion triggered. Distance to target: " + currentDistance);
                AddReward(1.0f - (currentDistance / explosionDistanceThreshold));
                EndEpisode();
                Destroy(gameObject);
            }

            if (currentDistance > Math.Pow(detectionDistance, 1.5))
            {
                Debug.Log("Target too far, ending episode. Current distance: " + currentDistance);
                AddReward(-1.0f);
                EndEpisode();
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
            
            EndEpisode();
            Destroy(gameObject);
        }
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

        EndEpisode();
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isEnemyTarget(other.attachedRigidbody))
        {
            Debug.Log("Trigger entered with target missile.");
            AddReward(1.0f);
        }
        else
        {
            Debug.Log("Trigger entered with non-target object.");
            AddReward(-1f);
        }

        EndEpisode();
        Destroy(gameObject);
    }

    private bool isEnemyTarget(Rigidbody rb)
    {
        return rb != null && rb == enemyMissileRb;
    }
}
