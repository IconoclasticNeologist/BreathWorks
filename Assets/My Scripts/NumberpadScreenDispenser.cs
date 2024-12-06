using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Playables;

public class NumberpadScreenDispenser : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject screen;
    [SerializeField] private TextMeshProUGUI screenText;
    [SerializeField] private Image screenImage;

    [Header("Display Settings")]
    [SerializeField] private string defaultMessage = "ENTER CODE";
    [SerializeField] private string accessMessage = "ACCESS GRANTED";

    [Header("Timeline Settings")]
    [SerializeField] private PlayableDirector successTimeline;
    [SerializeField] private bool resetTimelineBeforePlaying = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onAccessGranted;
    [SerializeField] private UnityEvent onDisplayReset;

    [Header("Optional Settings")]
    [SerializeField] private Color defaultTextColor = Color.black;
    [SerializeField] private Color accessGrantedColor = Color.green;

    private void Start()
    {
        // Ensure screen is visible and showing default message
        if (screen != null)
        {
            screen.SetActive(true);

            if (screenText != null)
            {
                screenText.text = defaultMessage;
                screenText.color = defaultTextColor;
            }
        }
    }

    public void DisplayAccessGranted()
    {
        if (screen != null)
        {
            screen.SetActive(true);

            if (screenText != null)
            {
                screenText.text = accessMessage;
                screenText.color = accessGrantedColor;
            }
        }

        // Play timeline if assigned
        if (successTimeline != null)
        {
            if (resetTimelineBeforePlaying)
            {
                successTimeline.time = 0;
            }
            successTimeline.Play();
        }

        // Invoke UnityEvent
        onAccessGranted?.Invoke();
    }

    public void ResetDisplay()
    {
        if (screen != null && screenText != null)
        {
            screenText.text = defaultMessage;
            screenText.color = defaultTextColor;
        }

        // Stop timeline if it's playing
        if (successTimeline != null)
        {
            successTimeline.Stop();
        }

        // Invoke reset event
        onDisplayReset?.Invoke();
    }

    // Keep this method for compatibility with other scripts
    public void DisplayScreen()
    {
        DisplayAccessGranted();
    }

    // Keep this method for compatibility with other scripts
    public void HideScreen()
    {
        ResetDisplay();
    }

    // Helper method to check if timeline is currently playing
    public bool IsTimelinePlaying()
    {
        return successTimeline != null && successTimeline.state == PlayState.Playing;
    }

    // Method to manually stop the timeline
    public void StopTimeline()
    {
        if (successTimeline != null)
        {
            successTimeline.Stop();
        }
    }
}