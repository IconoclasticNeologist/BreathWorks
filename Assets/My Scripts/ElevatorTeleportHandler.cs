using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ElevatorTeleportHandler : MonoBehaviour
{
    private TeleportationArea teleportArea;
    private GameObject xrOrigin;
    private ElevatorPlayerParent elevatorParent;

    private void Start()
    {
        xrOrigin = GameObject.Find("XR Origin");
        elevatorParent = GetComponent<ElevatorPlayerParent>();

        teleportArea = GetComponent<TeleportationArea>();
        if (teleportArea != null)
        {
            teleportArea.teleportationProvider.endLocomotion += OnTeleportationEnded;
        }
    }

    private void OnTeleportationEnded(LocomotionSystem locomotionSystem)
    {
        if (xrOrigin != null)
        {
            Vector3 playerPos = xrOrigin.transform.position;
            Vector3 elevatorPos = transform.position;

            // Using elevator's bounds for more accurate detection
            Bounds elevatorBounds = GetComponent<Collider>().bounds;
            float heightCheck = Mathf.Abs(playerPos.y - elevatorPos.y);

            // Check if player is within elevator bounds and at a reasonable height
            if (elevatorBounds.Contains(playerPos) ||
                (IsPointWithinXZ(playerPos, elevatorBounds) && heightCheck < 1f))
            {
                xrOrigin.transform.SetParent(transform, true);
            }
        }
    }

    private bool IsPointWithinXZ(Vector3 point, Bounds bounds)
    {
        return point.x >= bounds.min.x && point.x <= bounds.max.x &&
               point.z >= bounds.min.z && point.z <= bounds.max.z;
    }

    private void OnDestroy()
    {
        if (teleportArea != null && teleportArea.teleportationProvider != null)
        {
            teleportArea.teleportationProvider.endLocomotion -= OnTeleportationEnded;
        }
    }
}