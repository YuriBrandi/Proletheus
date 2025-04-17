using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    public TMP_Text fpsText;
    public float updateInterval = 0.5f;

    private int frames = 0;
    private float timer = 0f;

    void Update()
    {
        frames++;
        timer += Time.unscaledDeltaTime;

        if (timer >= updateInterval)
        {
            int fps = Mathf.RoundToInt(frames / timer);
            fpsText.text = $"FPS: {fps}";
            frames = 0;
            timer = 0f;
        }
    }
}