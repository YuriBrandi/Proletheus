using UnityEngine;
using System.Collections;

public class BoidController : MonoBehaviour
{
    public GameObject boidPrefab;

    public int spawnCount = 10;

    public float spawnRadius = 4.0f;

    [Range(0.1f, 20.0f)]
    public float velocity = 6.0f;

    [Range(0.0f, 0.9f)]
    public float velocityVariation = 0.5f;

    [Range(0.1f, 20.0f)]
    public float rotationCoeff = 4.0f;

    [Range(0.1f, 10.0f)]
    public float neighborDist = 2.0f;

    public LayerMask searchLayer;

    private GameObject Flock;

    void Start()
    {
        
        //Create Flock parent
        Flock = new GameObject("Flock");
        Flock.transform.position = this.transform.position;
        
        for (var i = 0; i < spawnCount; i++) Spawn();
        boidPrefab.SetActive(false); //Hide once all objects are spawned
    }

    public GameObject Spawn()
    {
        return Spawn(transform.position + Random.insideUnitSphere * spawnRadius);
    }

    public GameObject Spawn(Vector3 position)
    {
        var rotation = Quaternion.Slerp(transform.rotation, Random.rotation, 0.3f);
        var boid = Instantiate(boidPrefab, position, rotation) as GameObject;

        //Set parent
        boid.transform.SetParent(Flock.transform, true);
        
        if (boid.GetComponent<BoidBehaviour>())
            boid.GetComponent<BoidBehaviour>().controller = this;
        /*if (boid.GetComponent<BoidBehaviour_Test>())
            boid.GetComponent<BoidBehaviour_Test>().controller = this;*/
        return boid;
    }

    // Make the flock follow the manager's behaviour.
    void OnDestroy()
    {
        if (Flock != null)
        {
            Debug.Log("Flock is being destroyed");
            Destroy(Flock);
        }
    }

    void OnDisable()
    {
        if (Flock != null)
            Flock.SetActive(false);
    }

    void OnEnable()
    {
        if (Flock != null)
            Flock.SetActive(true);
    }
}
