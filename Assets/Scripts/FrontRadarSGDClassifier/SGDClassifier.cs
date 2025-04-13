using UnityEngine;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Newtonsoft.Json;

public class SGDClassifier : MonoBehaviour
{
    [System.Serializable]
    public class ModelData
    {
        public float[] weights;
        public float bias;
    }

    [Header("JSON File (Used for Inference only)")]
    public TextAsset jsonFile;
    private float[] weights;
    private float bias;

    void Awake()
    {
        if (isEnabled())
        {
            // Carica i pesi dal JSON
            ModelData model = JsonConvert.DeserializeObject<ModelData>(jsonFile.text);
            weights = model.weights;
            bias = model.bias;
            Debug.Log("Modello SGD caricato");
        }
    }

    public bool isEnabled()
    {
        return jsonFile != null;
    }

    public int Predict(float[] features)
    {
        float dot = 0f;
        for (int i = 0; i < weights.Length; i++)
            dot += features[i] * weights[i];

        float score = dot + bias;
        float prob = 1f / (1f + Mathf.Exp(-score)); // sigmoid

        return prob >= 0.5f ? 1 : 0;
    }
}
