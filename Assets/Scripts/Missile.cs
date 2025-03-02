using System;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

public class Missile : MonoBehaviour
{
    [Header("Trip Settings")]
    private Vector3 startPoint;
    private Vector3 endPoint;
    private Vector3 controlPoint;
    private float speed;
    private float t = 0f;


    public void Initialize(Vector3 startPoint_, Vector3 endPoint_, float speed_, float height_)
    {
        // Usa un nome univoco basato sul tempo per evitare conflitti
        this.name = "enemy_missile_" + DateTime.Now.ToString("HHmmssfff");

        // Imposta il missile come questo oggetto
        this.transform.SetParent(this.transform);

        startPoint = startPoint_;
        endPoint = endPoint_;

        Debug.Log("Missile End Point (XZ): " + endPoint);

        // Applica al punto di partenza
        startPoint.y = height_;

        // Calcola il punto di controllo per l'arco (il punto centrale è sollevato)
        Vector3 midPoint = (startPoint + endPoint) / 2;
        controlPoint = midPoint + Vector3.up * height_;

        speed = speed_;

        // Aggiungi un controllo per assicurarti che le posizioni non siano NaN
        if (float.IsNaN(startPoint.x) || float.IsNaN(startPoint.y) || float.IsNaN(startPoint.z) ||
            float.IsNaN(endPoint.x) || float.IsNaN(endPoint.y) || float.IsNaN(endPoint.z) ||
            float.IsNaN(controlPoint.x) || float.IsNaN(controlPoint.y) || float.IsNaN(controlPoint.z))
        {
            Debug.LogError("Invalid position values detected.");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Increment progress based on speed and distance
        t += Time.deltaTime * speed / Vector3.Distance(startPoint, endPoint);

        // Clamp t to 1 to avoid overshooting
        t = Mathf.Clamp01(t);

        print("StartPoint: " + startPoint + " | ControlPoint: " + controlPoint + " | endPoint: " + endPoint);

        // Calculate position using quadratic B�zier curve (parabolic path)
        Vector3 position = Mathf.Pow(1 - t, 2) * startPoint
                           + 2 * (1 - t) * t * controlPoint
                           + Mathf.Pow(t, 2) * endPoint;

        this.transform.position = position;

        // Rotate missile to face movement direction using tangent
        if (t < 1)
        {
            Vector3 tangent = 2 * (1 - t) * (controlPoint - startPoint) + 2 * t * (endPoint - controlPoint);
            this.transform.forward = tangent.normalized;
        }
        else
        {
            enabled = false; // Stop updating once the target is reached
        }
    }
}
