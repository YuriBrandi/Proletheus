using UnityEngine;
using UnityEngine.VFX;

public class ExplosionSpawner : MonoBehaviour
{
    public VisualEffectAsset explosionEffect;
    public float explosionSize = 1f;
    public float vfxLifetime = 3f;

    public void SpawnExplosionVFX(Vector3 position)
    {
        GameObject vfxObject = new GameObject("ExplosionVFX");
        vfxObject.transform.position = position;
        vfxObject.transform.localScale = Vector3.one * explosionSize;

        VisualEffect vfx = vfxObject.AddComponent<VisualEffect>();
        vfx.visualEffectAsset = explosionEffect;

        Destroy(vfxObject, vfxLifetime);
    }

}
