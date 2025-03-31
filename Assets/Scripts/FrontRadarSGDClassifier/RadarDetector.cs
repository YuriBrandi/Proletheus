using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;
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
    
    [Header("Verify TAG (used for training only)")]
    public string missileTag;

    [Header("Training Spawners")]
    public MissileSpawner missileSpawner;
    public AircraftSpawner aircraftSpawner;

    [Header("Training Socket Client (used for training only)")]
    public TrainerSocketClient classifier;

    [Header("Training Settings")]
    public float spawnInterval = 3f;

    [Header("Inference Model")]
    public SGDClassifier sgdClassifier;

    [Header("Advanced Settings (will override inference model)")]
    public bool deterministicClassification = false;

    [Header("Debugging")]
    public bool colorObjects = true;

    private float timer = 0f;

    const int IS_ENEMY_VALUE = 1;
    const int NOT_ENEMY_VALUE = 0;

    /*
     * k: gameObject.GetInstanceID();
     * v: [0: NOT_ENEMY_VALUE | 1: IS_ENEMY_VALUE]
     * isEmpty(k) -> no decisions
     */
    private Dictionary<int, int> decisionMap = new Dictionary<int, int>();

    void Start()
    {
        if (deterministicClassification)
            Debug.Log("Modalità Deterministica ATTIVA");
        else if (sgdClassifier.isEnabled())
            Debug.Log("Modalità Inferenza ATTIVA");
        else
        {
            if (classifier == null)
                Debug.LogError("A SocketClient must be assigned.");

            Debug.Log("Modalità Training ATTIVA");
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
    }

    void FixedUpdate()
    {
        if (!sgdClassifier.isEnabled() && !deterministicClassification)
        {
            timer += Time.fixedDeltaTime;

            if (timer >= spawnInterval)
            {
                timer = 0f;
                spawnFlyingInstance();
            }
        }

        //------------------------------------------------------------------

        // Find all objects within detection range
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);

        var filteredHits = filterColliders(hits);

        if (filteredHits.Count > 0)
        {
            Collider observedHit = filteredHits.First();

            float[] features = ExtractFeatures(observedHit);
            int isEnemy = observedHit.CompareTag(missileTag) ? IS_ENEMY_VALUE : NOT_ENEMY_VALUE;

            int prediction;

            //Se stiamo facendo inferenza, deve richiamare il metodo predict
            if (deterministicClassification)
            {
                prediction = isEnemy;
                if (prediction == IS_ENEMY_VALUE)
                    InterceptorBehaviour.OnEnemyMissileDetected(observedHit.attachedRigidbody);
            }
            else if (sgdClassifier.isEnabled())
            {
                prediction = sgdClassifier.Predict(features);
                if (prediction == IS_ENEMY_VALUE)
                    InterceptorBehaviour.OnEnemyMissileDetected(observedHit.attachedRigidbody);
            }
            else //Altrimenti chiamare il metodo di predict di python
                prediction = classifier.RadarClassifyObject(features, isEnemy);

            if (colorObjects)
            {
                Renderer[] renderers = observedHit.GetComponentsInChildren<Renderer>();
                changeMaterialColor(renderers, prediction == NOT_ENEMY_VALUE  ? Color.green : Color.red);
            }

            // Logging/debug
            //Debug.Log($"{observedHit.name} → Prediction: {prediction} (Real: {isEnemy}) | " + (prediction == isEnemy ? "CORRECT" : "WRONG"));

            if(sgdClassifier.isEnabled() || deterministicClassification) // If in inference insert object in hashMap.
                decisionMap.Add(observedHit.gameObject.GetInstanceID(), isEnemy);
            else //Se fa training, deve cancellare il gameObject hit
            {
                if(isEnemy == IS_ENEMY_VALUE)
                    Destroy(observedHit.gameObject);
                else
                {
                    //Debug.Log("Sto per cancellare il gameobject: " + observedHit.gameObject.GetInstanceID());
                    //AircraftSpawner.TriggerFlyingTripEnd(observedHit.gameObject);
                    //Debug.Log("Disabling aircraft: " + observedHit.gameObject.GetInstanceID());
                    observedHit.gameObject.SetActive(false);
                }
            }                              
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
                if ((sgdClassifier.isEnabled() || deterministicClassification) && decisionMap.ContainsKey(hit.gameObject.GetInstanceID()))
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
