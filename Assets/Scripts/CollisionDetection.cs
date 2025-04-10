using UnityEngine;
using System.Collections; //Per coroutine

public class CollisionDetection : MonoBehaviour
{
    [Tooltip("Set 0 to disable")]
    public float sphereDiam = 200f; // Radius of the red circle
    public float animationDuration = .5f;
    public string missileTag;
    public Material collisionMat;

    private Vector3 missilePosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == missileTag)
        {
            missilePosition = col.transform.position;
            Destroy(col.gameObject);
            if (sphereDiam > 0f)
                DrawHitSphere(missilePosition, sphereDiam);

            CurriculumDebug.OnEnemyMissileDestroyed(false);
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
