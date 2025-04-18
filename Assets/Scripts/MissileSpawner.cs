using System.Security.Cryptography;
using Unity.MLAgents;
using Unity.VisualScripting;
using UnityEngine;

public class MissileSpawner : MonoBehaviour
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
    public MeshCollider targetCollider;

    [Header("City Corners")]
    public Transform A;   // Angle A
    public Transform B;   // Angle B
    public Transform C;   // Angle C
    public Transform D;   // Angle D

    [Header("Training Settings")]
    [Tooltip("Set to 0 to disable")]
    public int spawnRateSeconds = 0;

    [Header("Debug Settings")]
    public bool drawMissilesGizmos = false;

    [Header("Optional Attached Camera")]
    public SecondaryCamera enemyCamera;

    private float timer = 0f;



    private const float PARAMETER_DIRECTION_RANDOM = 4f; //Check defenceMissile_Curricula.yaml for RANDOM value (4 [aircraft off] and 5 [aircraft on])

    void Start()
    {
        //SpawnMissile();
    }

    public void FixedUpdate()
    {
        if (spawnRateSeconds > 0)
        {
            // Training mode, check for curriculum parameter.
            float trainingDirection = Academy.Instance.EnvironmentParameters.GetWithDefault("enemy_training_phases", -1f);

            if(trainingDirection >= 0)
            {
                // >= means 4 or 5
                if(trainingDirection >= PARAMETER_DIRECTION_RANDOM)
                    this.chosenDirection = Direction.RANDOM;
                else
                    this.chosenDirection = (Direction)trainingDirection;

                //Debug.Log($"Direction set to: {this.chosenDirection}");
            }

            timer += Time.fixedDeltaTime;

            if (timer >= spawnRateSeconds)
            {
                timer = 0f;
                SpawnMissile();
            }
        }
    }

    public void SpawnMissile()
    {
        if(missilePrefab == null) {
            print("MissilePrefab is null");
            return; 
        }

        GameObject missileObject = Instantiate(missilePrefab, GetPointByDirection(chosenDirection, offset), Quaternion.identity);

        // Attach Missile script
        Missile missileScript = missileObject.AddComponent<Missile>();

        // Randomizez height and velocity
        float height = UnityEngine.Random.Range(minHeight, maxHeight);

        // Initialize the missile with the needed parameters
        missileScript.Initialize(GetPointByDirection(chosenDirection, offset), GetRandomPointInCollider(targetCollider), height, drawMissilesGizmos);

        // If there is a secondary cam, use it to attach the missile.
        if (enemyCamera != null)
            enemyCamera.AttachCamTo(missileObject);
    }

    Vector3 GetRandomPointInCollider(MeshCollider collider)
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

        if (direction == Direction.RANDOM)
            direction = (Direction)Random.Range(0, (int) Direction.RANDOM); // Exclude "RANDOM" itself
        else
            if(Academy.Instance.EnvironmentParameters.GetWithDefault("enemy_training_phases", -1f) >= 0)
            {
                // Adjust based on direction (only curriculum training mode)
                direction = (Direction)Random.Range(0, (int) direction+1); //+1 because we want to include current direction itself
            }
      

        Vector3 result = Vector3.zero;

        switch (direction)
        {
            case Direction.N:
                result = Vector3.Lerp (B.position, C.position, 0.5f) + Vector3.forward * offset;
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

    public void setChosenDirection(int directionIndex)
    {
         if (directionIndex >= 0 && directionIndex < System.Enum.GetValues(typeof(Direction)).Length)
        {
            chosenDirection = (Direction)directionIndex;
        }
        else
        {
            Debug.LogWarning("Invalid direction index!");
        }
    }
}
