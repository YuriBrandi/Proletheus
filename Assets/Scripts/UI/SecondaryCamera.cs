using UnityEngine;
using UnityEngine.UI;

public class SecondaryCamera : MonoBehaviour
{
    public Vector3 camOffset = new Vector3(0, 5, -10);

    public float followSpeed = 5f;
    public RawImage referenceRawImage;
    public float cameraDisableDelay = 5f; // Better to be as long as the Explosion effect duration.

    private GameObject targetedObject;
    private float timeSinceLostTarget;

    void Start()
    {
        timeSinceLostTarget = cameraDisableDelay;
    }

    void LateUpdate()
    {
        if (targetedObject != null)
        {
            referenceRawImage.enabled = true;
            Vector3 desiredPos = targetedObject.transform.position + camOffset;
            this.transform.position = Vector3.Lerp(this.transform.position, desiredPos, Time.deltaTime * followSpeed);
            this.transform.LookAt(targetedObject.transform);
            timeSinceLostTarget = 0f;
        }
        else
        {
            timeSinceLostTarget += Time.deltaTime;

            if (timeSinceLostTarget >= cameraDisableDelay)
                referenceRawImage.enabled = false;
        }

    }

    public void AttachCamTo(GameObject _targetedObject)
    {
        if (_targetedObject != null)
        {
            targetedObject = _targetedObject;
        }
    }
}