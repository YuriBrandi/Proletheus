using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;


public class RadarAgent : Agent
{
    [Header("Radar Settings")]
    public float detectionRange = 50f;
    public float minAltitude = 10f; // Minimum height to consider an object
    public float minSpeed = 1f; // Minimum speed to consider an object moving
    public int maxObservedObjects = 10; // Fixed number of closest objects to observe
    [Header("Verify TAG (used for reward only)")]
    public string missileTag;
    public MissileController missileSpawner;
    bool isInference;

    /*
     * k: gameObject.GetInstanceID();
     * v: [false: not enemy | true: enemy]
     * isEmpty(k) -> no decisions
     */
    private Dictionary<int, bool> decisionMap = new Dictionary<int, bool>();

    void Start()
    {
        isInference = !Academy.Instance.IsCommunicatorOn;
        if (isInference)
        {
            Debug.Log("Modalità INFERENZA attiva.");
        }   
        else
        {
            Debug.Log("Modalità TRAINING attiva.");
        }
    }



    public override void OnEpisodeBegin()
    {
        if (!isInference)
            missileSpawner.SpawnMissile();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Find all objects within detection range
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);

        var filteredHits = filterColliders(hits);

        // Add observations for the closest objects
        foreach (var hit in filteredHits)
        {
            // Add relative position (normalized)
            Vector3 relativePos = hit.transform.position - transform.position;
            sensor.AddObservation(relativePos.normalized);

            // Add normalized distance
            sensor.AddObservation(relativePos.magnitude / detectionRange);

            // Add velocity information
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                sensor.AddObservation(rb.linearVelocity.normalized);
                sensor.AddObservation(rb.linearVelocity.magnitude / 100f); // Assuming max speed 100
            }
            else
            {
                sensor.AddObservation(Vector3.zero);
                sensor.AddObservation(0f);
            }
        }

        // Pad with empty observations if fewer than maxObservedObjects are detected
        for (int i = filteredHits.Count; i < maxObservedObjects; i++)
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Find all objects within detection range
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);

        var filteredHits = filterColliders(hits);

        float totalReward = 0f;

        // Evaluate actions for the closest objects
        for (int i = 0; i < filteredHits.Count; i++)
        {
            var hit = filteredHits[i];
            int prediction = actions.DiscreteActions[i]; // Access discrete actions

            // Use tag only for reward calculation and debugging
            bool isEnemy = hit.CompareTag(missileTag);
            bool correctPrediction = (prediction == 1 && isEnemy) || (prediction == 0 && !isEnemy);

            // Calculate reward with distance-based scaling
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            float distanceReward = Mathf.Clamp01(distance / detectionRange);

            Renderer[] renderers = hit.GetComponentsInChildren<Renderer>();

            // Debug log for correctness
            if (correctPrediction)
            {
                changeMaterialColor(renderers, Color.yellow);
                Debug.Log($"Correctly classified object at distance {distance:0.00} (Reward: {0.1f + distanceReward * 0.1f})");
            }
            else
            {
                changeMaterialColor(renderers, Color.blue);
                Debug.Log($"Incorrectly classified object at distance {distance:0.00} (Penalty: -0.2f)");
            }

            totalReward += correctPrediction ? 0.1f + distanceReward * 0.1f : -0.2f;

            if (!isInference) //Se fa training, deve cancellare il gameObject hit
                Destroy(hit.gameObject);
            else
            {
                Debug.Log("ADD " + hit.gameObject.GetInstanceID());
                decisionMap.Add(hit.gameObject.GetInstanceID(), isEnemy);
            }
        }

        AddReward(totalReward);

        if (!isInference)
            EndEpisode();
    }

    private void changeMaterialColor(Renderer[] renderers, Color color)
    {
        foreach (Renderer renderer in renderers)
        {
            // Modifica il colore del materiale
            renderer.material.color = color;
        }
    }
    private List<Collider> filterColliders(Collider[] hits)
    {
        // Filter objects based on movement and altitude
        return hits
            .Where(hit =>
            {
                if (isInference && decisionMap.ContainsKey(hit.gameObject.GetInstanceID()))
                    return false;

                // Check if the object is high enough
                bool isHighEnough = hit.transform.position.y >= minAltitude;

                // Check if the object is moving
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                bool isMoving = rb != null && rb.linearVelocity.magnitude >= minSpeed;

                return isHighEnough && isMoving;
            })
            .OrderBy(hit => Vector3.Distance(transform.position, hit.transform.position))
            .Take(maxObservedObjects)
            .ToList();
    }

    /*public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Optional: Implement human heuristic for testing
        // This example uses random decisions
        var discreteActions = actionsOut.DiscreteActions;
        for (int i = 0; i < discreteActions.Length; i++)
        {
            discreteActions[i] = Random.Range(0, 2);
        }
    }*/

    private void OnDrawGizmosSelected()
    {

        // Visualize the radar radius in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}