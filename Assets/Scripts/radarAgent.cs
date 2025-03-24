using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Random = UnityEngine.Random;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using System;


public class RadarAgent : Agent
{
    [Header("Radar Settings")]
    public float detectionRange = 50f;
    public float minAltitude = 10f; // Minimum height to consider an object
    public float minSpeed = 1f; // Minimum speed to consider an object moving
    public int maxObservedObjects = 10; // Fixed number of closest objects to observe
    
    [Header("Verify TAG (used for reward only)")]
    public string missileTag;

    [Header("Training Spawners")]
    public MissileSpawner missileSpawner;
    public AircraftSpawner aircraftSpawner;

    [Header("Training Settings")]
    public int actionsPerEpisode = 5;

    [Header("Debugging")]
    public bool colorObjects = true;

    private int spawnedFlyingInstances;
    private bool isInference;

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
            Debug.Log("INFERENCE MODE.");
        }   
        else
        {
            Debug.Log("TRAINING MODE.");
        }
    }



    public override void OnEpisodeBegin()
    {
        spawnedFlyingInstances = 0;
        spawnFlyingInstance();
    }

    /*
        Should work in training only.
        Will spawn a random flying object in a balanced manner (50% enemy/ 50% friendly)
    */
    public void spawnFlyingInstance()
    {
        if (!isInference)
        {
            if (missileSpawner == null || aircraftSpawner == null)
            {
                Debug.LogError("A missileSpawner and an aircraftSpawner both need to be assigned.");
            }
            else
            {
                // Flip a coin
                if (Random.Range(0, 2) == 0)
                {
                    missileSpawner.SpawnMissile();
                }
                else
                {
                    var flyingPrefabs = aircraftSpawner.flyingPrefabs;
                    
                    int randInd = Random.Range(0, flyingPrefabs.Length);

                    aircraftSpawner.SpawnAircraft(flyingPrefabs[randInd]);


                }

                /*var flyingPrefabs = aircraftSpawner.flyingPrefabs;

                int randInd = Random.Range(0, flyingPrefabs.Length + 3);

                if (randInd < flyingPrefabs.Length)
                     aircraftSpawner.SpawnAircraft(flyingPrefabs[randInd]);
                else
                   missileSpawner.SpawnMissile();*/
                

            }

            spawnedFlyingInstances++;

        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Find all objects within detection range
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);

        var filteredHits = filterColliders(hits);

        if (filteredHits.Count > 0)
        {
            Collider observedHit = filteredHits.First();

            // Add relative position (normalized)
            Vector3 relativePos = observedHit.transform.position - transform.position;
            sensor.AddObservation(relativePos.normalized);

            // Add normalized distance
            sensor.AddObservation(relativePos.magnitude / detectionRange);

            // Add velocity information
            Rigidbody rb = observedHit.GetComponent<Rigidbody>();
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

            // Add BoxCollider size information.
            BoxCollider bc = observedHit.GetComponent<BoxCollider>();

            if (bc != null)
                sensor.AddObservation(bc.size);
            else
                sensor.AddObservation(Vector3.zero);
        }
        else //PAD
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
            sensor.AddObservation(Vector3.zero);
        }
        
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Find all objects within detection range
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);

        var filteredHits = filterColliders(hits);

        if (filteredHits.Count > 0)
        {

            Collider hit = filteredHits.First();

            float prediction = actions.ContinuousActions[0]; // Access discrete actions

            //Debug.Log(prediction);

            // Use tag only for reward calculation and debugging
            bool isEnemy = hit.CompareTag(missileTag);
            bool correctPrediction = (prediction > 0 && isEnemy) || (prediction <= 0 && !isEnemy);;

            Renderer[] renderers = hit.GetComponentsInChildren<Renderer>();


            if(colorObjects) //enemy prediction : red else green
                changeMaterialColor(renderers, prediction == 0  ? Color.green : Color.red);

            // Debug log for correctness
            if (correctPrediction)
            {
                Debug.Log($"Correctly classified object " + hit.gameObject.name + " (" + hit.gameObject.GetInstanceID() + ") " + "(Reward: +1f)");
                Debug.Log($"CORRECT PREDICTION");

                Debug.Log($"CORRECT PREDICTION " + (isEnemy ? "MISSILE" : "AIRCRAFT"));
            }

            else
            {
                Debug.Log($"Incorrectly classified object " + hit.gameObject.name + " (" + hit.gameObject.GetInstanceID() + ") " + "(Penalty: -1f)");
                Debug.Log($"WRONG PREDICTION");

                Debug.Log($"WRONG PREDICTION " + (isEnemy ? "MISSILE" : "AIRCRAFT"));
            }


            AddReward(correctPrediction ? 0.2f : -0.2f);
            
            if (isInference) // If in inference insert object in hashMap.
            {
                Debug.Log("ADD " + hit.gameObject.GetInstanceID());
                decisionMap.Add(hit.gameObject.GetInstanceID(), isEnemy);
            }
            else //Se fa training, deve cancellare il gameObject hit
            {
                if (isEnemy) // If is enemy (tag:missile)
                    Destroy(hit.gameObject);
                else // If non-enemy (aircraft)
                {
                    //Debug.Log("Sto per cancellare il gameobject: " + hit.gameObject.GetInstanceID());
                    //AircraftSpawner.TriggerFlyingTripEnd(hit.gameObject);
                    Debug.Log("Disabling aircraft: " + hit.gameObject.GetInstanceID());
                    hit.gameObject.SetActive(false);
                    
                }
            }


            if (!isInference)
                if (spawnedFlyingInstances == actionsPerEpisode)
                    EndEpisode();
                else
                    spawnFlyingInstance();

        }

 
    }

    private void changeMaterialColor(Renderer[] renderers, Color color)
    {
        foreach (Renderer renderer in renderers)
        {
            // Chhange mat color
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


    private void OnDrawGizmosSelected()
    {

        // Visualize the radar radius in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}