using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;
using System.Collections; // Needed for IEnumerator
using System.Collections.Generic;
using System;
using UnityEngine.Timeline;

public class RadarDetector : MonoBehaviour
{
    [Header("Radar Settings")]
    public float detectionRange = 50f;
    public float minAltitude = 10f; // Minimum height to consider an object
    public float minSpeed = 1f; // Minimum speed to consider an object moving
    public int maxObservedObjects = 10; // Fixed number of closest objects to observe
    public int maxHashSetSize = 1000; //Number of stored entries in the HashSet at once (determines cleanup)
    public bool transmitToInterceptor = true;
    
    [Header("Verify TAG (used for training only)")]
    public string missileTag;
    public string aircraftTag;
    public string defenceTag;

    [Header("Training Settings")]
    public TrainerSocketClient trainingClassifier;
    [Tooltip("Values > 0 will trigger self-managed spawn. This should be 0 unless for testing.")]
    public float spawnInterval = 0f;

    [Header("Training Spawners")]
    public MissileSpawner missileSpawner;
    public AircraftSpawner aircraftSpawner;

    [Header("Inference Model")]
    public SGDClassifier sgdClassifier;

    [Header("Advanced Settings (will override inference model)")]
    public bool deterministicClassification = false;

    private float trainingTimer = 0f;

    const int IS_ENEMY_VALUE = 1;
    const int NOT_ENEMY_VALUE = 0;

    /*
        No need to track decisions, only check existance in O(1).
        GameObjectsID have no guarantee of not being recylced. Need to use GameObject pointers instead.
        Need to perform occasional cleanup.
     */
    private HashSet<GameObject> decidedObjects = new HashSet<GameObject>();

    void Start()
    {
        if (deterministicClassification)
            Debug.Log("Modalità Deterministica ATTIVA");
        else if (sgdClassifier.isEnabled())
            Debug.Log("Modalità Inferenza ATTIVA");
        else
        {
            if (trainingClassifier == null)
                Debug.LogError("A SocketClient must be assigned.");

            Debug.Log("Modalità Training ATTIVA");
        }

        StartCoroutine(autoHashSetCleanup());
    }

    void FixedUpdate()
    {
        if (!sgdClassifier.isEnabled() && !deterministicClassification && spawnInterval > 0)
        {
            trainingTimer += Time.fixedDeltaTime;

            if (trainingTimer >= spawnInterval)
            {
                trainingTimer = 0f;
                spawnFlyingInstance();
            }
        }

        //------------------------------------------------------------------

        // Find all objects within detection range
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);

        var filteredHits = filterColliders(hits);

        //if (filteredHits.Count > 0)
        foreach (Collider observedHit in filteredHits)
        {
            //Collider observedHit = filteredHits.First();

            float[] features = extractFeatures(observedHit);
            int isEnemy = observedHit.CompareTag(missileTag) ? IS_ENEMY_VALUE : NOT_ENEMY_VALUE;

            int prediction;

            //Debug.Log("Collided with: " + observedHit.gameObject.name);

            //Se stiamo facendo inferenza, deve richiamare il metodo predict
            if (deterministicClassification)
            {
                prediction = isEnemy;
                if (transmitToInterceptor && prediction == IS_ENEMY_VALUE)
                    InterceptorBehaviour.OnEnemyMissileDetected(observedHit.attachedRigidbody);
            }
            else if (sgdClassifier.isEnabled())
            {
                prediction = sgdClassifier.Predict(features);
                if (transmitToInterceptor && prediction == IS_ENEMY_VALUE)
                    InterceptorBehaviour.OnEnemyMissileDetected(observedHit.attachedRigidbody);
            }
            else //Altrimenti chiamare il metodo di predict di python
                prediction = trainingClassifier.RadarClassifyObject(features, isEnemy);
            
            assignDebugLabel(observedHit.gameObject, prediction);

            // Logging/debug
            Debug.Log($"{observedHit.name} → Prediction: {prediction} (Real: {isEnemy}) | " + (prediction == isEnemy ? "CORRECT" : "WRONG"));

            if (spawnInterval > 0) // If testing classifier with self-spawn, destroy objects (PHASE 1)
                Destroy(observedHit.gameObject);
            else 
            {
                // This is both for Inference and normal Training (PHASE 2-3)
                decidedObjects.Add(observedHit.gameObject);
            }                              
        }

    }

    float[] extractFeatures(Collider col)
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
    
    private void assignDebugLabel(GameObject hitObject, int prediction)
    {
        if (hitObject.CompareTag(missileTag))
            hitObject.GetComponent<Missile>().setRadarLabel(prediction);
        else if (hitObject.CompareTag(aircraftTag))
            hitObject.GetComponent<Aircraft>().setRadarLabel(prediction);
        else if (hitObject.CompareTag(defenceTag))
            hitObject.GetComponent<DefenceMissileAgent>().setRadarLabel(prediction);
        else
            Debug.LogError("Untagged flying object, this should not happen.");
    }

    private List<Collider> filterColliders(Collider[] hits)
    {
        // Filter objects based on movement and altitude
        return hits
            .Where(hit =>
            {
                if (!(spawnInterval > 0) && decidedObjects.Contains(hit.gameObject))
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

    /*
        Only needed to test classificator performance, not really needed for final training.
    */
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
    }


    // Coroutine for cleanup
    IEnumerator autoHashSetCleanup()
    {
        while (true)
        {
            if (decidedObjects.Count > maxHashSetSize)
            {
                decidedObjects.RemoveWhere(obj => obj == null);
                Debug.Log("Performing HashSet cleanup");
            }

            yield return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the radar radius in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

}
