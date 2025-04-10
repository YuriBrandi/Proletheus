using UnityEngine;

public class rotateEngine : MonoBehaviour
{
    [Header("Attached Engine Objects")]
    public EngineData[] engines; // Struct di engine

    [System.Serializable]
    public struct EngineData
    {
        public GameObject engine;
        public float speed;
        public Axis axis;
    }

    public enum Axis
    {
        X,
        Y,
        Z
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var engineData in engines)
        {
            if (engineData.engine != null)
            {
                // Rotate the engine based on the selected axis and speed
                switch (engineData.axis)
                {
                    case Axis.X:
                        engineData.engine.transform.Rotate(engineData.speed * Time.deltaTime, 0, 0);
                        break;
                    case Axis.Y:
                        engineData.engine.transform.Rotate(0, engineData.speed * Time.deltaTime, 0);
                        break;
                    case Axis.Z:
                        engineData.engine.transform.Rotate(0, 0, engineData.speed * Time.deltaTime);
                        break;
                }
            }
        }
    }
}

