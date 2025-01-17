using UnityEngine;

public class MissileBehaviour : MonoBehaviour
{
    private Rigidbody rb;

    // Tempo iniziale di stabilizzazione (in secondi)
    private float stabilizationTime = 0.5f;

    private float timeSinceLaunch;

    // Inizializzazione
    void Start()
    {
        // Recupera il componente Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Il missile non ha un componente Rigidbody!");
        }

        // Inizializza il tempo
        timeSinceLaunch = 0f;
    }

    // Aggiorna l'orientamento del missile
    void FixedUpdate()
    {
        // Aggiorna il tempo trascorso
        timeSinceLaunch += Time.fixedDeltaTime;

        // Controlla se il Rigidbody è presente
        if (rb != null)
        {
            // Ottieni la velocità del missile
            Vector3 velocity = rb.linearVelocity;

            // Se la velocità è significativa
            if (velocity.sqrMagnitude > 0.1f)
            {
                // Dopo il periodo di stabilizzazione, orienta il missile verso la velocità
                if (timeSinceLaunch > stabilizationTime)
                {
                    transform.rotation = Quaternion.LookRotation(velocity);
                }
            }
        }
    }
}
