using UnityEngine;
using UnityEngine.VFX;
using System.Collections; //Per coroutine

public class CollisionDetection : MonoBehaviour
{
    [Tooltip("Set to null to disable")]
    public string missileTag;
    public VisualEffectAsset explosionEffect;
    public float explosionSize = 30f;
    public float vfxLifetime = 5f;

    private Vector3 impactPoint;

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == missileTag)
        {
            impactPoint = col.transform.position;
            Destroy(col.gameObject);
            if (explosionEffect != null)
                SpawnExplosionVFX(impactPoint);

            CurriculumDebug.OnEnemyMissileDestroyed(false);
        }
    }

    void SpawnExplosionVFX(Vector3 position)
    {
        GameObject vfxObject = new GameObject("ExplosionVFX");
        vfxObject.transform.position = position;
        vfxObject.transform.localScale = Vector3.one * explosionSize;

        VisualEffect vfx = vfxObject.AddComponent<VisualEffect>();
        vfx.visualEffectAsset = explosionEffect;

        // Automatically destroy the VFX object when it finishes
        StartCoroutine(DestroyVFXAfterDelay(vfxObject, vfxLifetime));
    }

    IEnumerator DestroyVFXAfterDelay(GameObject vfxObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(vfxObject);
    }
}
