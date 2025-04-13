using UnityEngine;
using TMPro;

public class MenuScreen : MonoBehaviour
{
    [Header("Menu Settings")]
    public Material blurMat;
    public float transitionDuration = 1f;
    public float initialBlurValue = 2f;

    [Header("Menu Canvas")]
    public GameObject canvasParent;
    
    [Header("Starting Text")]
    public TextMeshProUGUI targetText;
    public float beatSpeed = 2f;           // Speed of pulsing
    public float minAlpha = 0.2f;          // Minimum transparency

    [Header("Orbit Camera")]
    public OrbitCamera cameraScript;

    private Color originalTextColor;

    private float distance;
    private Transform target;

    private float currentX = 0f;
    private float currentY = 20f;

    private bool isTransitioning = false;
    private float transitionProgress = 0f;

    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        if (blurMat == null)
        {
            Debug.LogWarning("[MenuScreen] Blur material is not assigned.");
            return;
        }

        if (!blurMat.HasProperty("_Blur"))
        {
            Debug.LogWarning("[MenuScreen] The material does not have a 'Blur' property.");
            return;
        }
            
        
        if (cameraScript == null)
        {
            Debug.LogWarning("[MenuScreen] Orbit camera is not assigned.");
            return;
        }

        if (targetText == null)
        {
            Debug.LogWarning("[TMProBeatController] No TextMeshProUGUI assigned.");
            return;
        }

        blurMat.SetFloat("_Blur", initialBlurValue); // Ensure fully unblurred

        distance = cameraScript.getCameraDistance();
        target = cameraScript.getTarget();

        cameraScript.enabled = false; // Momentarily disable camera orbiting

        originalTextColor = targetText.color;



    }

       

    void Update()
    {
        
        if (!isTransitioning)
        {
            float t = (Mathf.Sin(Time.time * beatSpeed) + 1f) / 2f;
            float alpha = Mathf.Lerp(minAlpha, originalTextColor.a, t);

            targetText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, alpha);

            if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                // Begin transition
                isTransitioning = true;
                canvasParent.SetActive(false);
                startPos = transform.position;
                startRot = transform.rotation;
                transitionProgress = 0f;
            }
        }
        else
        {
            Quaternion desiredRotation = Quaternion.Euler(currentY, currentX, 0);
            Vector3 desiredPosition = target.position + desiredRotation * new Vector3(0, 0, -distance);

            if (isTransitioning)
            {
                transitionProgress += Time.deltaTime / transitionDuration;

                // Interpolate position and rotation
                transform.position = Vector3.Lerp(startPos, desiredPosition, transitionProgress);
                transform.rotation = Quaternion.Slerp(startRot, Quaternion.LookRotation(target.position - transform.position), transitionProgress);

                // Animate blur 
                float currentBlur = Mathf.Lerp(initialBlurValue, 0f, transitionProgress);
                blurMat.SetFloat("_Blur", currentBlur);

                // End transition
                if (transitionProgress >= 1f)
                {
                    isTransitioning = false;
                    cameraScript.enabled = true;
                    this.enabled = false;
                }
            }
        }



    }
}
