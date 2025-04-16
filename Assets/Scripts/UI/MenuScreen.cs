using UnityEngine;
using TMPro;

public class MenuScreen : MonoBehaviour
{
    [Header("Menu Settings")]
    public Material blurMat;
    public float transitionDuration = 1f;
    public float initialBlurValue = 2f;

    [Header("UI References")]
    public GameObject settingsUI;
    public GameObject startMenuUI;
    public GameObject actionsUI;
    public GameObject creditsUI;
    public GameObject debugUI;

    [Header("Starting Text")]
    public TextMeshProUGUI targetText;
    public float beatSpeed = 2f;
    public float minAlpha = 0.2f;

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

    private bool hasShownStartUI = false;

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
            Debug.LogWarning("[MenuScreen] No TextMeshProUGUI assigned.");
            return;
        }

        if (settingsUI == null)
        {
            Debug.LogWarning("[MenuScreen] SettingsUI not assigned.");
            return;
        }

        if (actionsUI == null)
        {
            Debug.LogWarning("[MenuScreen] ActionsUI not assigned.");
            return;
        }

        if (creditsUI == null)
        {
            Debug.LogWarning("[MenuScreen] CreditsUI not assigned.");
            return;
        }

        blurMat.SetFloat("_Blur", initialBlurValue);  // Ensure fully blurred

        distance = cameraScript.getCameraDistance();
        target = cameraScript.getTarget();
  
        cameraScript.enabled = false;  // Momentarily disable camera orbiting
        originalTextColor = targetText.color;

        settingsUI.SetActive(false);
        Time.timeScale = 1f;

        actionsUI.SetActive(false);
        creditsUI.SetActive(false);
        debugUI.SetActive(false);

        if (startMenuUI != null)
            startMenuUI.SetActive(true);
    }

    void Update()
    {
        if (!isTransitioning)
        {
            // Welcome text heartbeat.
            float t = (Mathf.Sin(Time.time * beatSpeed) + 1f) / 2f;
            float alpha = Mathf.Lerp(minAlpha, originalTextColor.a, t);
            targetText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, alpha);

            if ((Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && !hasShownStartUI)
            {
                // Begin transition
                isTransitioning = true;
                hasShownStartUI = true;

                if (startMenuUI != null)
                {
                    Destroy(startMenuUI);
                    startMenuUI = null;
                }

                // Animate blur 
                settingsUI.SetActive(false);
                Time.timeScale = 1f;

                // End transition
                startPos = transform.position;
                startRot = transform.rotation;
                transitionProgress = 0f;
            }
        }
        else
        {
            Quaternion desiredRotation = Quaternion.Euler(currentY, currentX, 0);
            Vector3 desiredPosition = target.position + desiredRotation * new Vector3(0, 0, -distance);

            transitionProgress += Time.unscaledDeltaTime / transitionDuration;

            // Interpolate position and rotation
            transform.position = Vector3.Lerp(startPos, desiredPosition, transitionProgress);
            transform.rotation = Quaternion.Slerp(startRot, Quaternion.LookRotation(target.position - transform.position), transitionProgress);

            float currentBlur = Mathf.Lerp(initialBlurValue, 0f, transitionProgress);
            blurMat.SetFloat("_Blur", currentBlur);

            if (transitionProgress >= 1f)
            {
                isTransitioning = false;
                cameraScript.enabled = true;
                actionsUI.SetActive(true);
                //enabled = false;
            }         
        }

        if (Input.GetKeyDown(KeyCode.Escape) && !isTransitioning)
        {
            toggleSettingsUI();
        }
    }

    public void toggleSettingsUI()
    {
        if(creditsUI.activeSelf)
            creditsUI.SetActive(false);

        if (!settingsUI.activeSelf)
        {
            settingsUI.SetActive(true);
            actionsUI.SetActive(false);
            blurMat.SetFloat("_Blur", initialBlurValue);
            cameraScript.enabled = false;
            Time.timeScale = 0f; // Pause the Scene
        }
        else
        {
            settingsUI.SetActive(false);
            actionsUI.SetActive(true);
            blurMat.SetFloat("_Blur", 0f);
            cameraScript.enabled = true;
            Time.timeScale = 1f; // Resume the scene
        }
    }

    public void OnGithubButtonPressed()
    {
        Application.OpenURL("https://github.com/YuriBrandi/Proletheus");
    }

    public void toggleCreditsUI()
    {
        if (!creditsUI.activeSelf)
        {
            settingsUI.SetActive(false);
            creditsUI.SetActive(true);
        }
        else
        {
            settingsUI.SetActive(true);
            creditsUI.SetActive(false);
        }
    }

    public void toggleDebugUI(bool value)
    {
        debugUI.SetActive(value);
    }


}
