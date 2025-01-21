using UnityEngine;

public class SecondaryCameraFollow : MonoBehaviour
{
    public string targetName = "TargetName"; // The name of the target to find
    public Vector3 offset = new Vector3(0, 10, -10); // Offset from the target position

    private Transform target; // The object the camera will follow

    void Start()
    {
        FindTarget(); // Attempt to find the target at the start
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Set the camera position to follow the target with the offset
            transform.position = target.position + offset;
            
            // Make the camera look at the target, but maintain the camera's rotation
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, target.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
        else
        {
            FindTarget(); // If no target, search for one
        }
    }

    void FindTarget()
    {
        // Use GameObject.Find to find the first object by name
        GameObject foundTarget = GameObject.Find(targetName);

        // If the object is found, set it as the target
        if (foundTarget != null)
        {
            target = foundTarget.transform;
        }
    }
}
