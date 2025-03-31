using System;
using UnityEngine;

public class InterceptorBehaviour : MonoBehaviour
{
    [Header("Settings")]
    public GameObject defenceMissilePrefab;

    public static event Action<Rigidbody> EnemyMissileDetected;
    public static event Action<float, float> MinDistanceDebug;

    private float minDistance = 1000.0f;

    public void Start()
    {
        EnemyMissileDetected += LaunchDefenceMissile;
        MinDistanceDebug += SetMinDistanceDebug;
    }

    public static void OnEnemyMissileDetected(Rigidbody enemyMissileRb)
    {
        EnemyMissileDetected.Invoke(enemyMissileRb);
    }

    public static void OnMinDistanceDebug(float distance, float explodeSignal)
    {
        MinDistanceDebug.Invoke(distance, explodeSignal);
    }

    private void SetMinDistanceDebug(float distance, float explodeSignal)
    {
        if (distance < minDistance)
        {
            minDistance = distance;

            Debug.Log("MIN DISTANCE: " + minDistance + " | explodeSignal: " + explodeSignal);
        }
    }

    // Lancia un missile agente verso il missile nemico rilevato
    private void LaunchDefenceMissile(Rigidbody enemyMissileRb)
    {
        // Istanzia un missile agente orientato verso l'alto
        var missileAgent = Instantiate(defenceMissilePrefab, gameObject.transform.position, Quaternion.LookRotation(Vector3.up));

        // Inizializza l'agente con il missile nemico target
        var agent = missileAgent.GetComponent<DefenceMissileAgent>();
        agent.Initialize(enemyMissileRb);
    }
}
