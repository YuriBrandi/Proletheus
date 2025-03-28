using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;
using System.Collections.Generic;
using System;

public class RadarDetector : MonoBehaviour
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

    [Header("Training Socket Client")]
    public TrainerSocketClient classifier;

    [Header("Training Settings")]
    public int parallelFlyingObjects = 5;

    [Header("Debugging")]
    public bool colorObjects = true;

    private int spawnedFlyingInstances;

    void Start()
    {
        if (classifier == null)
             Debug.LogError("A SockerClient must be assigned.");
        else
        {
            spawnedFlyingInstances = 0;
            for (int i = 0; i < parallelFlyingObjects; i++)
                spawnFlyingInstance();

        }
    }

     public void spawnFlyingInstance()
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

        }

        spawnedFlyingInstances++;
    }

    void FixedUpdate()
    {
        // Find all objects within detection range
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);

        var filteredHits = filterColliders(hits);

        if (filteredHits.Count > 0)
        {
            Collider observedHit = filteredHits.First();

            float[] features = ExtractFeatures(observedHit);
            bool isEnemy = observedHit.CompareTag(missileTag) ? true : false;

            bool prediction = classifier.RadarClassifyObject(features, isEnemy);

            if(colorObjects)
            {
                Renderer[] renderers = observedHit.GetComponentsInChildren<Renderer>();
                changeMaterialColor(renderers, prediction == false  ? Color.green : Color.red);
            }

            // Logging/debug
            Debug.Log($"{observedHit.name} → Prediction: {prediction} (Real: {isEnemy})");

            if (isEnemy) // If is enemy (tag:missile)
                Destroy(observedHit.gameObject);
            else // If non-enemy (aircraft)
            {
                //Debug.Log("Sto per cancellare il gameobject: " + observedHit.gameObject.GetInstanceID());
                //AircraftSpawner.TriggerFlyingTripEnd(observedHit.gameObject);
                //Debug.Log("Disabling aircraft: " + observedHit.gameObject.GetInstanceID());
                observedHit.gameObject.SetActive(false);
                
            }

            spawnFlyingInstance();


                               
        }
        else return;
    }

    float[] ExtractFeatures(Collider col)
    {
        Vector3 relativePos = col.transform.position - transform.position;

        // Can't be null due to filterColliders()
        Vector3 relVel = col.attachedRigidbody.linearVelocity;

        Renderer renderer = col.GetComponentInChildren<Renderer>();
        Vector3 size = renderer != null ? renderer.bounds.size : Vector3.zero;

        return new float[]
        {
            relativePos.normalized.x,
            relativePos.normalized.y,
            relativePos.normalized.z,
            relativePos.magnitude / detectionRange,
            relVel.normalized.x,
            relVel.normalized.y,
            relVel.normalized.z,
            relVel.magnitude / 100f,
            size.x, size.y, size.z
        };
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
                /*if (isInference && decisionMap.ContainsKey(hit.gameObject.GetInstanceID()))
                    return false;*/

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
