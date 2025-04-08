using System;
using UnityEngine;
using Random = UnityEngine.Random;

// Used only to call Academy
using Unity.MLAgents;

public class AircraftSpawner : MonoBehaviour
{
    [Header("Global Aircraft Settings")]
    public GameObject[] flyingPrefabs;
    public float offset = 200f; // Origin offset (makes missile appear from farther).

    [Header("City Corners")]
    public Transform A;
    public Transform B;
    public Transform C;
    public Transform D;

    [Header("Additional Settings")]
    public bool disableAutoSpawn = false;
    public bool drawGizmos = false;

    //private int trainingPhaseRateSeconds = 5;
    //private float timer = 0f;

    private const float SPAWN_PARAMETER = 5f;

    public static event Action<GameObject> OnFlyingTripEnd;

    public void Start()
    {
        if(Academy.Instance.IsCommunicatorOn)
        {
            // Check for curriculum parameter (will default to inspector if not present)
            disableAutoSpawn = Academy.Instance.EnvironmentParameters.GetWithDefault("enemy_training_phases", Convert.ToSingle(this.disableAutoSpawn)) != SPAWN_PARAMETER;
        }

        if(!disableAutoSpawn)
        {
            foreach (var flyingObject in flyingPrefabs)   
                SpawnAircraft(flyingObject);
            
            OnFlyingTripEnd += HandleAircraftRespawn;
        }

    }

    /*private void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;

        if (timer >= trainingPhaseRateSeconds)
        {
            timer = 0f;

            // Check for curriculum parameter (will default to inspector if not present)
            disableAutoSpawn = Academy.Instance.EnvironmentParameters.GetWithDefault("enemy_training_phases", Convert.ToSingle(this.disableAutoSpawn)) != SPAWN_PARAMETER;
        }
    }*/


    public static void TriggerFlyingTripEnd(GameObject referenceObject)
    {
        OnFlyingTripEnd?.Invoke(referenceObject);
    }

    public void SpawnAircraft(GameObject referenceObject)
    {
        if (referenceObject == null)
            Debug.LogError("Referencing a null flying object");
            
        Vector3 startPoint, endPoint;

        GetRandomPoints(out startPoint, out endPoint);

        GameObject aircraftObject = Instantiate(referenceObject, startPoint, Quaternion.identity);
        aircraftObject.name = referenceObject.name;

        Aircraft aircraftScript = aircraftObject.GetComponent<Aircraft>();
        aircraftScript.setCoords(startPoint, endPoint);

        if (drawGizmos)
            aircraftScript.enableGizmos(true);

        aircraftObject.SetActive(true);

        //Debug.Log("Aircraft ID: " +  aircraftObject.GetInstanceID());
    }

    void HandleAircraftRespawn(GameObject aircraftObject)
    {
        GameObject referenceObject = getReferenceFromAircraft(aircraftObject);
        Destroy(aircraftObject);
        SpawnAircraft(referenceObject);
    }
    
    private void GetRandomPoints(out Vector3 start, out Vector3 end)
    {
        Vector3[] side1 = { A.position, B.position };
        Vector3[] side2 = { B.position, C.position };
        Vector3[] side3 = { C.position, D.position };
        Vector3[] side4 = { D.position, A.position };
        Vector3[][] sides = { side1, side2, side3, side4 };

        int chosenSideIndex = Random.Range(0, sides.Length);
        Vector3[] chosenSide = sides[chosenSideIndex];
        addOffset(chosenSide);
        start = GetRandomPointOnLine(chosenSide[0], chosenSide[1]);

        int oppositeSideIndex = (chosenSideIndex + 2) % sides.Length;
        Vector3[] oppositeSide = sides[oppositeSideIndex];
        addOffset(oppositeSide);
        end = GetRandomPointOnLine(oppositeSide[0], oppositeSide[1]);
    }

    private Vector3 GetRandomPointOnLine(Vector3 start, Vector3 end)
    {
        float t = Random.Range(0f, 1f);
        return Vector3.Lerp(start, end, t);
    }

    private GameObject getReferenceFromAircraft(GameObject aircraftObject)
    {

        foreach (GameObject prefab in flyingPrefabs)
        {
            if (prefab.name == aircraftObject.name)
                return prefab;
        }

        Debug.LogError("No reference from aircraftObject. This should not happen.");
        return null;
    }

    private void addOffset(Vector3[] point)
    {
        if (point.Length != 2)
            return;

        for (int i = 0; i < point.Length; i++)
        {
            if (point[i] == B.position)
            {
                point[i] += new Vector3(-offset, 0, offset);
            }
            else if (point[i] == A.position)
            {
                point[i] += new Vector3(-offset, 0, -offset);
            }
            else if (point[i] == C.position)
            {
                point[i] += new Vector3(offset, 0, offset);
            }
            else if (point[i] == D.position)
            {
                point[i] += new Vector3(offset, 0, -offset);
            }
        }
    }
}
