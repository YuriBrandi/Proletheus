using UnityEngine;

public class Missile : MonoBehaviour
{
    public float radius = 50f;         // Raggio della parabola circolare
    public float speed = 0.1f;         // Velocità del missile
    public float heightMultiplier = 30f; // Fattore di altezza per la parabola
    public float rotationSpeed = 100f; // Velocità di rotazione per il missile
    public Vector3 direction = Vector3.forward; // Direzione principale del movimento

    private float angle = 0f;          // Angolo corrente nella traiettoria

    void Start()
    {
        // Impostiamo la posizione iniziale a (0, 0, 500)
        transform.position = new Vector3(0, 500, 0);
    }

    void Update()
    {
        // Aggiorna l'angolo in base alla velocità e al tempo
        angle += speed * Time.deltaTime;

        // Calcola le coordinate della traiettoria orizzontale (su X e Z)
        float x = Mathf.Cos(angle) * radius;      // Posizione lungo l'arco circolare in X
        float z = Mathf.Sin(angle) * radius;      // Posizione lungo l'arco circolare in Z

        // Calcola l'altezza come una funzione del tempo, in base a heightMultiplier
        float y = Mathf.Sin(angle) * heightMultiplier; // Aumentato per far volare più in alto

        // Aggiorna la posizione del missile
        Vector3 newPosition = new Vector3(x, y, z);
        transform.position = newPosition;

        // Calcoliamo la direzione del movimento
        Vector3 movementDirection = newPosition - transform.position;

        if (movementDirection != Vector3.zero)
        {
            // Calcoliamo la rotazione in base alla direzione di movimento
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);

            // Applichiamo la rotazione in modo fluido
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
