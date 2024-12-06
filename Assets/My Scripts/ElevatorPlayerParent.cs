using UnityEngine;

public class ElevatorPlayerParent : MonoBehaviour
{
    private GameObject xrOrigin;
    private CharacterController characterController;
    private bool isParented = false;
    private BoxCollider elevatorBounds;

    private void Start()
    {
        xrOrigin = GameObject.Find("XR Origin");
        if (xrOrigin != null)
        {
            characterController = xrOrigin.GetComponent<CharacterController>();
        }

        // Get the elevator's bounds
        elevatorBounds = GetComponent<BoxCollider>();
    }

    private bool IsPlayerInElevator()
    {
        if (xrOrigin == null || elevatorBounds == null) return false;

        // Convert player position to local space
        Vector3 localPoint = transform.InverseTransformPoint(xrOrigin.transform.position);

        // Get box collider's local bounds
        Vector3 halfSize = elevatorBounds.size * 0.5f;

        // Check if player is within bounds
        return Mathf.Abs(localPoint.x) < halfSize.x &&
               Mathf.Abs(localPoint.z) < halfSize.z;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isParented &&
            (other.gameObject == xrOrigin ||
            (characterController != null && other.gameObject == characterController.gameObject)))
        {
            if (IsPlayerInElevator())
            {
                ParentPlayer();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isParented &&
            (other.gameObject == xrOrigin ||
            (characterController != null && other.gameObject == characterController.gameObject)))
        {
            if (IsPlayerInElevator())
            {
                ParentPlayer();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isParented &&
            (other.gameObject == xrOrigin ||
            (characterController != null && other.gameObject == characterController.gameObject)))
        {
            UnparentPlayer();
        }
    }

    private void ParentPlayer()
    {
        xrOrigin.transform.SetParent(transform, true);
        isParented = true;
    }

    private void UnparentPlayer()
    {
        xrOrigin.transform.SetParent(null);
        isParented = false;
    }

    // Additional safety check
    private void LateUpdate()
    {
        if (isParented && !IsPlayerInElevator())
        {
            UnparentPlayer();
        }
    }
}