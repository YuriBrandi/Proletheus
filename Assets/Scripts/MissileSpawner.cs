using System.Security.Cryptography;
using Unity.MLAgents;
using Unity.VisualScripting;
using UnityEngine;

public class MissileController : MonoBehaviour
{
    public enum Direction
    {
        N, S, E, W, NE, NW, SE, SW, RANDOM
    }

    [Header("Global Missile Settings")]
    public GameObject missilePrefab;
    public float minHeight = 5f; // Minimum height for the arc
    public float maxHeight = 15f; // Maximum height for the arc
    public float offset = 200f; // Origin offset (makes missile appear from farther).
    public Direction chosenDirection;
    public BoxCollider targetCollider;

    [Header("City Corners")]
    public Transform A;   // Angle A
    public Transform B;   // Angle B
    public Transform C;   // Angle C
    public Transform D;   // Angle D

    public MissileCounter missileCounter;

    void Start()
    {
        //SpawnMissile();
    }

    public void SpawnMissile()
    {
        if(missilePrefab == null) {
            print("MissilePrefab is null");
            return; 
        }

        // Instanzia il missile prefab
        GameObject missileObject = Instantiate(missilePrefab, GetPointByDirection(chosenDirection, offset), Quaternion.identity);

        // Aggiungi lo script Missile al GameObject istanziato
        Missile missileScript = missileObject.AddComponent<Missile>();

        // Randomizza la velocità e l'altezza
        float height = UnityEngine.Random.Range(minHeight, maxHeight);

        // Inizializza il missile con i parametri necessari
        missileScript.Initialize(GetPointByDirection(chosenDirection, offset), GetRandomPointInBoxCollider(targetCollider), height);

        missileCounter.missileCounterIncrement();
    }

    Vector3 GetRandomPointInBoxCollider(BoxCollider collider)
    {
        // Get center and size of box collider
        Vector3 center = collider.bounds.center;
        Vector3 size = collider.bounds.size;

        // Get a random X and Z coordinate
        float randomX = Random.Range(center.x - size.x / 2, center.x + size.x / 2);
        float randomZ = Random.Range(center.z - size.z / 2, center.z + size.z / 2);

        return new Vector3(randomX, center.y, randomZ);
    }

    private Vector3 GetPointByDirection(Direction direction, float offset)
    {
        // Adjust based on direction
        if (direction == Direction.RANDOM)
        {
            direction = (Direction)Random.Range(0, 8); // Exclude "RANDOM" itself
        }

        Vector3 result = Vector3.zero;

        switch (direction)
        {
            case Direction.N:
                result = Vector3.Lerp(B.position, C.position, 0.5f) + Vector3.forward * offset;
                break;
            case Direction.S:
                result = Vector3.Lerp(A.position, D.position, 0.5f) + Vector3.back * offset;
                break;
            case Direction.E:
                result = Vector3.Lerp(C.position, D.position, 0.5f) + Vector3.right * offset;
                break;
            case Direction.W:
                result = Vector3.Lerp(A.position, B.position, 0.5f) + Vector3.left * offset;
                break;
            case Direction.NE:
                result = B.position + new Vector3(-offset, 0, offset);
                break;
            case Direction.NW:
                result = A.position + new Vector3(-offset, 0, -offset);
                break;
            case Direction.SE:
                result = C.position + new Vector3(offset, 0, offset);
                break;
            case Direction.SW:
                result = D.position + new Vector3(offset, 0, -offset);
                break;
        }

        return result;
    }
}
