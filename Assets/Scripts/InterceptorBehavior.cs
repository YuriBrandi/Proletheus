using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InterceptorBehaviour : MonoBehaviour
{
    [Header("Settings")]
    public GameObject defenceMissilePrefab;
    public float launchInterval = 0.5f;
    public GameObject spawnPoint;

    [Header("Optional Attached Camera")]
    public SecondaryCamera defenceCamera;

    // Non dovrebbe servire più
    //public static event Action<Rigidbody> EnemyMissileDetected;

    private static InterceptorBehaviour[] interceptors;
    private float lastLaunchTime = 0f;
    private Queue<Rigidbody> enemyMissileQueue = new Queue<Rigidbody>();

    public void Start()
    {
        //EnemyMissileDetected += LaunchDefenceMissile;


    }

    public void FixedUpdate()
    {
        // If queue has elements launch with interval
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
        {
            Debug.Log("Populating intercpetors array");
            interceptors = FindObjectsByType<InterceptorBehaviour>(FindObjectsSortMode.None);
        }


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

        Debug.Log("Requesting launch from " + nearestIntercept.name);

        //EnemyMissileDetected.Invoke(enemyMissileRb);
        //nearestIntercept.LaunchDefenceMissile(enemyMissileRb);

        nearestIntercept.RequestMissileLaunch(enemyMissileRb);
    }

    

    private void RequestMissileLaunch(Rigidbody enemyMissileRb)
    {
        this.enemyMissileQueue.Enqueue(enemyMissileRb);
    }

    // Lancia un missile agente verso il missile nemico rilevato
    private void LaunchDefenceMissile(Rigidbody enemyMissileRb)
    {

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.transform.position : transform.position;

        // Istanzia un missile agente orientato verso l'alto
        GameObject missileAgent = Instantiate(defenceMissilePrefab, spawnPos, Quaternion.LookRotation(Vector3.up));

        // Inizializza l'agente con il missile nemico target
        var agent = missileAgent.GetComponent<DefenceMissileAgent>();
        agent.Initialize(enemyMissileRb);

        if (defenceCamera != null)
            defenceCamera.AttachCamTo(missileAgent);
    }
}