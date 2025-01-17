using UnityEngine;
using System.Collections;

public class BoidController : MonoBehaviour
{
    public GameObject boidPrefab;

    public GameObject parentObject;

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

    void Start()
    {
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
        boid.transform.SetParent(parentObject.transform, true);
        
        if (boid.GetComponent<BoidBehaviour>())
            boid.GetComponent<BoidBehaviour>().controller = this;
        /*if (boid.GetComponent<BoidBehaviour_Test>())
            boid.GetComponent<BoidBehaviour_Test>().controller = this;*/
        return boid;
    }
}
