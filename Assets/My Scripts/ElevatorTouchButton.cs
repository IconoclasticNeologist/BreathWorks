using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Playables;  // Add this for PlayableDirector

public class ElevatorTouchButton : XRBaseInteractable
{
    [Header("Timeline Settings")]
    [SerializeField] public PlayableDirector descentTimeline;

    [Header("Visual Feedback")]
    [SerializeField] public Material normalMaterial;
    [SerializeField] public Material pressedMaterial;
    [SerializeField] public MeshRenderer buttonMeshRenderer;

    [Header("Audio Feedback")]
    [SerializeField] public AudioClip buttonPressSound;

    private AudioSource audioSource;
    private bool isPressed = false;

    protected override void Awake()
    {
        base.Awake();

        // Get mesh renderer if not assigned
        if (buttonMeshRenderer == null)
            buttonMeshRenderer = GetComponent<MeshRenderer>();

        // Set up audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;

        // Initial material setup
        if (buttonMeshRenderer != null && normalMaterial != null)
        {
            buttonMeshRenderer.material = normalMaterial;
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        PressButton();
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        ReleaseButton();
    }

    private void PressButton()
    {
        if (isPressed) return;

        isPressed = true;

        // Visual feedback
        if (buttonMeshRenderer != null && pressedMaterial != null)
        {
            buttonMeshRenderer.material = pressedMaterial;
        }

        // Audio feedback
        if (buttonPressSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonPressSound);
        }

        // Play descent timeline
        if (descentTimeline != null)
        {
            descentTimeline.time = 0;
            descentTimeline.Play();
            Debug.Log("Playing descent timeline");
        }
        else
        {
            Debug.LogWarning("Descent Timeline not assigned to button!", this);
        }
    }

    private void ReleaseButton()
    {
        if (!isPressed) return;

        isPressed = false;

        // Reset visual state
        if (buttonMeshRenderer != null && normalMaterial != null)
        {
            buttonMeshRenderer.material = normalMaterial;
        }
    }
}