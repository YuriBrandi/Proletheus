using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InterceptorBehaviour : MonoBehaviour
{
    [Header("Settings")]
    public GameObject defenceMissilePrefab;
    private float launchInterval = 1f;

    // Non dovrebbe servire più
    //public static event Action<Rigidbody> EnemyMissileDetected;
    public static event Action<float, float> MinDistanceDebug;

    private float minDistance = 1000.0f;

    private int defenceMissileCounter = 0;
    private HashSet<float> minDistanceSet = new HashSet<float>();

    private static InterceptorBehaviour[] interceptors;
    private float lastLaunchTime = 0f;
    private Queue<Rigidbody> enemyMissileQueue = new Queue<Rigidbody>();

    public void Start()
    {
        //EnemyMissileDetected += LaunchDefenceMissile;
        MinDistanceDebug += SetMinDistanceDebug;


    }

    public void FixedUpdate()
    {
        if (enemyMissileQueue.Count > 0 && (Time.time - lastLaunchTime >= launchInterval))
        {
            
            LaunchDefenceMissile(enemyMissileQueue.Dequeue());
            lastLaunchTime = Time.time;

        }
    }

    public static void OnEnemyMissileDetected(Rigidbody enemyMissileRb)
    {
        // Find all active objects with the InterceptorBehaviour script
        // Only the first call populates the array
        if(interceptors == null)
            interceptors = FindObjectsByType<InterceptorBehaviour>(FindObjectsSortMode.None);

        InterceptorBehaviour nearestIntercept = null;
        float minDist = Mathf.Infinity;

        foreach (InterceptorBehaviour intercept in interceptors)
        {
            /*
                Similar to magnitude but avoids the slow calls to the Sqrt.
                This measure is more perfect for simple distance comparising.
            */
            float newDist = (intercept.transform.position - enemyMissileRb.position).sqrMagnitude;
            if(newDist < minDist)
            {
                minDist = newDist;
                nearestIntercept = intercept;
            }
        }

        //EnemyMissileDetected.Invoke(enemyMissileRb);
        //nearestIntercept.LaunchDefenceMissile(enemyMissileRb);
        nearestIntercept.RequestMissileLaunch(enemyMissileRb);
    }

    public static void OnMinDistanceDebug(float distance, float explodeSignal)
    {
        MinDistanceDebug.Invoke(distance, explodeSignal);
    }

    private void SetMinDistanceDebug(float distance, float explodeSignal)
    {
        if(distance <= 180.0)
        {
            minDistanceSet.Add(distance); 

            Debug.Log("DISTANCE: " + minDistance + " | explodeSignal: " + explodeSignal + " | Accuracy : " + (float) minDistanceSet.Count / defenceMissileCounter * 100f + "%");
        }

        if (distance < minDistance)
        {
            minDistance = distance;

            Debug.Log("MIN DISTANCE: " + minDistance + " | explodeSignal: " + explodeSignal);
        }
    }

    private void RequestMissileLaunch(Rigidbody enemyMissileRb)
    {
        this.enemyMissileQueue.Enqueue(enemyMissileRb);
    }

    // Lancia un missile agente verso il missile nemico rilevato
    private void LaunchDefenceMissile(Rigidbody enemyMissileRb)
    {

        // Istanzia un missile agente orientato verso l'alto
        var missileAgent = Instantiate(defenceMissilePrefab, gameObject.transform.position, Quaternion.LookRotation(Vector3.up));

        // Inizializza l'agente con il missile nemico target
        var agent = missileAgent.GetComponent<DefenceMissileAgent>();
        agent.Initialize(enemyMissileRb);

        defenceMissileCounter++;
    }
}