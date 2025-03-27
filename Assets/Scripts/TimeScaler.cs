using UnityEngine;

public class TimeScaler : MonoBehaviour
{
    [Range(1f, 100f)]
    public float simulationSpeed = 5f;

    void Awake()
    {
        Time.timeScale = simulationSpeed;
        Time.fixedDeltaTime = 0.02f / simulationSpeed;
        Debug.Log($"Simulation speed: {simulationSpeed}x");
    }
}
