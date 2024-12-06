using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TouchButton : XRBaseInteractable
{
    [SerializeField] private int buttonNumber;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material touchedMaterial;
    [SerializeField] private NumberPad linkedKeypad;
    [SerializeField] private float pressDistance = 0.003f;

    [Header("Interaction Settings")]
    [SerializeField] private Vector3 interactionSize = new Vector3(0.02f, 0.02f, 0.02f); // Small interaction area
    [SerializeField] private Vector3 colliderOffset = Vector3.zero; // Optional offset if needed

    [Header("Audio")]
    [SerializeField] private AudioClip buttonPressSound;
    [SerializeField] private AudioClip buttonReleaseSound;
    [SerializeField] [Range(0f, 1f)] private float volumeLevel = 0.5f;

    private MeshRenderer meshRenderer;
    private AudioSource audioSource;
    private Vector3 startPosition;
    private bool isPressed = false;
    private bool canPress = true;

    protected override void Awake()
    {
        base.Awake();
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = normalMaterial;
        startPosition = transform.localPosition;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f;
        audioSource.volume = volumeLevel;

        // Configure collider size
        var boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }
        boxCollider.size = interactionSize;
        boxCollider.center = colliderOffset;
        boxCollider.isTrigger = false;

        // Configure rigidbody
        var rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public override bool IsHoverableBy(IXRHoverInteractor interactor)
    {
        if (interactor is XRRayInteractor)
            return false;

        return base.IsHoverableBy(interactor);
    }

    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);

        if (canPress && !isPressed)
        {
            PressButton();
        }
    }

    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);
        ReleaseButton();
    }

    private void PressButton()
    {
        isPressed = true;
        meshRenderer.material = touchedMaterial;
        transform.localPosition = startPosition - new Vector3(0, pressDistance, 0);

        if (buttonPressSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonPressSound, volumeLevel);
        }

        if (linkedKeypad != null)
        {
            linkedKeypad.ButtonPressed(buttonNumber);
        }

        canPress = false;
    }

    private void ReleaseButton()
    {
        isPressed = false;
        meshRenderer.material = normalMaterial;
        transform.localPosition = startPosition;

        if (buttonReleaseSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonReleaseSound, volumeLevel);
        }

        canPress = true;
    }

    // This helps visualize the interaction area in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(colliderOffset, interactionSize);
    }
}