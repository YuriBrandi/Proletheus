using UnityEngine;
using System.Collections; //Per coroutine

public class CollisionDetection : MonoBehaviour
{
    public float sphereDiam = 200f; // Radius of the red circle
    public float animationDuration = .5f;
    public string missileTag;
    public Material collisionMat;

    public MissileCounter missileCounter;

    private Vector3 missilePosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

     void OnTriggerEnter(Collider other)
    {
        // Check if missile hit the target
        if (other.tag == missileTag)
        {

            missilePosition = other.transform.position;
            Destroy(other.gameObject);
            missileCounter.missileCounterDecrement();
            //DrawHitSphere(missilePosition, sphereDiam); TODO: scommentare quando completeremo il progetto e gestire come il caso dei missili (entità distinte)
            enabled = false;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == missileTag)
        {
            missilePosition = col.gameObject.transform.position;
            Destroy(col.gameObject);
            missileCounter.missileCounterDecrement();
            //DrawHitSphere(missilePosition, sphereDiam);
        }
    }

    void DrawHitSphere(Vector3 position, float diameter)
    {
        // Create a new sphere GameObject
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "hitRadiusSphere";

        Destroy(sphere.GetComponent<Collider>()); // Removes the default SphereCollider

        // Set the position of the sphere
        sphere.transform.position = position; // Centered at origin

        // Set the radius (scale because the default size is 1 unit)
        //sphere.transform.localScale = Vector3.one * diameter;
        sphere.transform.localScale = Vector3.zero;

        if (collisionMat != null)
        {
            // Apply the material to the sphere
            Renderer renderer = sphere.GetComponent<Renderer>();
            renderer.material = collisionMat;
        }
        else
        {
            Debug.LogError("Material not found!");
        }

        //Animazione crescente con Coroutine
        StartCoroutine(AnimazioneSfera(sphere, sphereDiam));
    }

    IEnumerator AnimazioneSfera(GameObject sphere, float targetRadius)
    {
        float elapsedTime = 0f; // Tempo
        Vector3 finalScale = Vector3.one * targetRadius;

        while (elapsedTime < animationDuration)
        {
            // Scala la sfera col tempo
            float t = elapsedTime / animationDuration;
            sphere.transform.localScale = Vector3.Lerp(Vector3.zero, finalScale, t);

            elapsedTime += Time.deltaTime;
            yield return null; // Attesa prossimo frame
        }

        // Elimina margine di approssimazione
        sphere.transform.localScale = finalScale;
    }
}
