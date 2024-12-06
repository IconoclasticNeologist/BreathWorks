using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class DoorHandle : MonoBehaviour
{
    [Header("Handle Settings")]
    [SerializeField] private float maxRotation = 90f;
    [SerializeField] private AudioClip handleTurnSound;
    [SerializeField] private Transform doorTransform; // Reference to the main door object

    private XRGrabInteractable grabInteractable;
    private AudioSource audioSource;
    private Vector3 initialHandleRotation;
    private Vector3 initialDoorRotation;
    private bool isEnabled = false;
    private bool doorIsOpen = false;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        audioSource = gameObject.AddComponent<AudioSource>();

        // Store initial rotations
        initialHandleRotation = transform.localEulerAngles;
        if (doorTransform != null)
        {
            initialDoorRotation = doorTransform.localEulerAngles;
        }

        // Configure grab interactable to prevent taking the handle off
        grabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
        grabInteractable.trackPosition = false; // Important! This prevents moving the handle
        grabInteractable.trackRotation = true;

        // Initially disabled
        SetHandleEnabled(false);
    }

    public void SetHandleEnabled(bool enabled)
    {
        isEnabled = enabled;
        grabInteractable.enabled = enabled;
    }

    private void Update()
    {
        if (!isEnabled || doorIsOpen) return;

        if (grabInteractable.isSelected)
        {
            // Get the rotational difference
            float currentYRotation = transform.localEulerAngles.y;
            float rotationAmount = Mathf.DeltaAngle(initialHandleRotation.y, currentYRotation);

            // Clamp rotation
            rotationAmount = Mathf.Clamp(rotationAmount, 0, maxRotation);

            // Apply clamped rotation to handle
            transform.localEulerAngles = new Vector3(
                initialHandleRotation.x,
                initialHandleRotation.y + rotationAmount,
                initialHandleRotation.z
            );

            // Check if handle has been turned enough to open door
            if (rotationAmount >= 45f && !doorIsOpen)
            {
                OpenDoor();
            }
        }
        else
        {
            // Reset handle rotation when released
            transform.localEulerAngles = initialHandleRotation;
        }
    }

    private void OpenDoor()
    {
        if (doorTransform != null && !doorIsOpen)
        {
            doorIsOpen = true;

            // Play sound
            if (audioSource != null && handleTurnSound != null)
            {
                audioSource.PlayOneShot(handleTurnSound);
            }

            // Animate the door opening
            StartCoroutine(AnimateDoorOpen());
        }
    }

    private System.Collections.IEnumerator AnimateDoorOpen()
    {
        float openTime = 1.0f;
        float elapsedTime = 0;
        Vector3 startRotation = doorTransform.localEulerAngles;
        Vector3 targetRotation = startRotation + new Vector3(0, -90, 0); // Open door 90 degrees

        while (elapsedTime < openTime)
        {
            elapsedTime += Time.deltaTime;
            float percentComplete = elapsedTime / openTime;

            // Use smoothstep for more natural movement
            float smoothPercent = percentComplete * percentComplete * (3 - 2 * percentComplete);

            doorTransform.localEulerAngles = Vector3.Lerp(startRotation, targetRotation, smoothPercent);

            yield return null;
        }

        // Ensure door ends at exact target rotation
        doorTransform.localEulerAngles = targetRotation;
    }
}