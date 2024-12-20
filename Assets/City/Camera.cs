using UnityEngine;

public class Camera : MonoBehaviour
{
    public Transform missile; // Il missile che la telecamera seguir�
    public Vector3 offset = new Vector3(0, 10, -20); // Posizione relativa della camera rispetto al missile
    public float smoothSpeed = 0.5f; // Velocit� di interpolazione della posizione della telecamera

    void LateUpdate()
    {
        // Calcola la posizione desiderata della telecamera
        Vector3 desiredPosition = missile.position + offset;

        // Lerp per rendere il movimento della telecamera pi� fluido
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Applica la nuova posizione alla telecamera
        transform.position = smoothedPosition;

        // Fai guardare la telecamera al missile
        transform.LookAt(missile);
    }
}
