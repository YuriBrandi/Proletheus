using System;
using Unity.MLAgents;
using UnityEngine;

public class Missile : MonoBehaviour
{
    [Header("Trip Settings")]
    private Vector3 startPoint;
    private Vector3 endPoint;
    private Vector3 controlPoint;

    private Rigidbody rb;

    public void Initialize(Vector3 startPoint_, Vector3 endPoint_, float height_)
    {
        // Usa un nome univoco basato sul tempo per evitare conflitti
        //this.name = "enemy_missile_" + DateTime.Now.ToString("HHmmssfff");
        this.name = "enemy_missile";

        // Aggiungi un componente Rigidbody per applicare la forza
        rb = gameObject.GetComponent<Rigidbody>();

        // Se il Rigidbody non è già presente, lo aggiungiamo
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;  // Attiva la gravità
        }

        startPoint = startPoint_;
        endPoint = endPoint_;

        // Applica al punto di partenza
        startPoint.y = height_;

        // Calcola il punto di controllo per l'arco (il punto centrale è sollevato)
        Vector3 midPoint = (startPoint + endPoint) / 2;
        controlPoint = midPoint + Vector3.up * height_;

        // Aggiungi un controllo per assicurarti che le posizioni non siano NaN
        if (float.IsNaN(startPoint.x) || float.IsNaN(startPoint.y) || float.IsNaN(startPoint.z) ||
            float.IsNaN(endPoint.x) || float.IsNaN(endPoint.y) || float.IsNaN(endPoint.z) ||
            float.IsNaN(controlPoint.x) || float.IsNaN(controlPoint.y) || float.IsNaN(controlPoint.z))
        {
            Debug.LogError("Invalid position values detected.");
            return;
        }

        gameObject.layer = LayerMask.NameToLayer("Missiles");

        // Calcola la velocità iniziale e lancia il missile
        LaunchMissile();
    }

    private void LaunchMissile()
    {
        // Calcola la direzione dal punto di partenza a quello finale
        Vector3 direction = (endPoint - startPoint).normalized;

        // Calcola la velocità iniziale necessaria per il lancio parabolico
        float gravity = Physics.gravity.y;  // Usare la gravità globale
        float distance = Vector3.Distance(startPoint, endPoint);
        float launchSpeed = Mathf.Sqrt(distance * -gravity / Mathf.Sin(2 * Mathf.Atan2(controlPoint.y - startPoint.y, distance)));

        // Imposta la velocità iniziale
        Vector3 velocity = direction * launchSpeed;

        // Aggiungi la forza alla Rigidbody
        rb.linearVelocity = velocity;

        // Correggi la posizione iniziale per evitare che il missile parta dalla posizione di default del Rigidbody
        transform.position = startPoint;
    }

    private void FixedUpdate()
    {

        if (transform.position.y < 0)
        {
            Debug.Log("[Missile]: start " + startPoint + " | end " + endPoint);
            Destroy(gameObject);
            return;
        }

        // Ottieni la direzione dalla velocità corrente del Rigidbody
        if (rb.linearVelocity.sqrMagnitude > 0.1f) // Controlla se il missile si sta muovendo
        {
            // Calcola la direzione del movimento
            Vector3 forwardDirection = rb.linearVelocity.normalized;

            // Allinea la rotazione dell'oggetto alla direzione del movimento
            Quaternion targetRotation = Quaternion.LookRotation(forwardDirection);

            // Applica una rotazione graduale per evitare rotazioni troppo brusche
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
}
