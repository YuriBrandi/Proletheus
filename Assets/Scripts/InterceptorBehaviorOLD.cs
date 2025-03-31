using UnityEngine;

public class MissileBattery : MonoBehaviour
{
    // Prefab del missile da istanziare
    public GameObject missilePrefab;

    // Velocità del missile
    public float missileSpeed = 100f;

    // Angolo di lancio (inclinazione)
    public float launchAngle = 45f;

    // Direzione di lancio (in gradi, 0 = avanti, 90 = destra, 180 = indietro, 270 = sinistra)
    public float launchDirection = 0f;

    // Metodo per lanciare un missile
    public void LaunchMissile()
    {
        // Calcolo della direzione di lancio in base all'angolo e alla direzione
        Vector3 direction = CalculateLaunchDirection(launchAngle, launchDirection);

        // Calcolo della rotazione del missile in base alla direzione
        Quaternion rotation = Quaternion.LookRotation(direction);

        // Istanziazione del missile con la rotazione calcolata
        GameObject missile = Instantiate(missilePrefab, transform.position, rotation);

        // Imposta la direzione del missile
        Rigidbody missileRigidbody = missile.GetComponent<Rigidbody>();
        if (missileRigidbody != null)
        {
            missileRigidbody.linearVelocity = direction * missileSpeed;
        }
        else
        {
            Debug.LogWarning("Il prefab del missile non ha un componente Rigidbody!");
        }
    }

    // Calcola la direzione del lancio basandosi su angolo e direzione
    private Vector3 CalculateLaunchDirection(float angle, float direction)
    {
        // Converte gli angoli in radianti
        float angleRad = angle * Mathf.Deg2Rad;
        float directionRad = direction * Mathf.Deg2Rad;

        // Calcolo della direzione del lancio
        Vector3 launchDirection = new Vector3(
            Mathf.Cos(directionRad) * Mathf.Cos(angleRad), // Componente X
            Mathf.Sin(angleRad),                           // Componente Y
            Mathf.Sin(directionRad) * Mathf.Cos(angleRad)  // Componente Z
        );

        return launchDirection.normalized;
    }

    // Metodo di test per il lancio manuale
    private void Update()
    {
        // Esempio di test: premi la barra spaziatrice per lanciare un missile
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LaunchMissile();
        }

        // Modifica direzione con i tasti freccia (opzionale per test dinamici)
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            launchDirection -= 1f;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            launchDirection += 1f;
        }

        // Clamp della direzione tra 0 e 360 per evitare valori fuori range
        launchDirection = launchDirection % 360f;
        if (launchDirection < 0) launchDirection += 360f;
    }
}
