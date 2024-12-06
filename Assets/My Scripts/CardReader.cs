using UnityEngine;
using UnityEngine.Playables;

public class CardReader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MeshRenderer redLight;
    [SerializeField] private MeshRenderer greenLight;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip failSound;
    [SerializeField] private PlayableDirector timeline;

    [Header("Settings")]
    [SerializeField] private bool allowMultipleSwipes = false;
    [SerializeField] private float resetDelay = 1f; // Time before reader resets for next swipe

    [Header("Materials")]
    [SerializeField] private Material redLightEmissive;
    [SerializeField] private Material redLightUnlit;
    [SerializeField] private Material greenLightEmissive;
    [SerializeField] private Material greenLightUnlit;

    private bool isActivated = false;

    private void Start()
    {
        if (redLight != null) redLight.material = redLightEmissive;
        if (greenLight != null) greenLight.material = greenLightUnlit;

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void HandleCardSwipe()
    {
        if (!isActivated || allowMultipleSwipes)
        {
            ActivateReader();
        }
    }

    private void ActivateReader()
    {
        isActivated = true;

        // Switch materials
        if (redLight != null) redLight.material = redLightUnlit;
        if (greenLight != null) greenLight.material = greenLightEmissive;

        // Play success sound
        if (audioSource != null && successSound != null)
        {
            audioSource.PlayOneShot(successSound);
        }

        // Play the timeline
        if (timeline != null)
        {
            timeline.time = 0;
            timeline.Play();
            Debug.Log("Timeline triggered by card swipe");
        }
        else
        {
            Debug.LogWarning("No timeline assigned to card reader!", this);
        }

        // If multiple swipes are allowed, reset after delay
        if (allowMultipleSwipes)
        {
            Invoke("ResetReader", resetDelay);
        }
    }

    private void ResetReader()
    {
        // Reset lights
        if (redLight != null) redLight.material = redLightEmissive;
        if (greenLight != null) greenLight.material = greenLightUnlit;

        if (allowMultipleSwipes)
        {
            isActivated = false;
        }
    }
}