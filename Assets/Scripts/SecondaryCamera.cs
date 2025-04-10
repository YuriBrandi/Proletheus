using UnityEngine;

public class SecondaryCameraFollow : MonoBehaviour
{
    public string targetName = "TargetName"; // The name of the target to find
    public Vector3 offset = new Vector3(20f, 10f, 0f); // Lateral offset from the target position (adjusted for side view)
    public float rotationSpeed = 5f; // Speed at which the camera rotates to follow the missile

    private Transform target; // The object the camera will follow

    void Start()
    {
        FindTarget(); // Attempt to find the target at the start
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Set the camera position to follow the target with the new lateral offset
            transform.position = target.position + offset;

            // Make the camera smoothly rotate to face the target's position
            Vector3 directionToTarget = target.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
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
