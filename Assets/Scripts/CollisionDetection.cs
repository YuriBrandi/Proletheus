using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    [Tooltip("Set to null to disable")]
    public string missileTag;

    [Header("Optional Explosion Spawner")]
    public ExplosionSpawner expSpawner;

    private Vector3 impactPoint;

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag(missileTag))
        {
            impactPoint = col.transform.position;
            Destroy(col.gameObject);

            if (expSpawner != null)
                expSpawner.SpawnExplosionVFX(impactPoint);

            CurriculumDebug.OnEnemyMissileDestroyed(false);
        }
    }

}
