using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Playables;

[AddComponentMenu("VR/Grab Trigger Action")]
public class GrabTriggerAction : XRGrabInteractable
{
    [Header("Timeline Settings")]
    [SerializeField] private PlayableDirector timelineDirector;
    [SerializeField] private bool playTimelineOnGrab = true;
    [SerializeField] private bool resetTimelineOnRelease = true;

    [Header("Object Visibility")]
    [SerializeField] private GameObject[] objectsToShow;
    [SerializeField] private GameObject[] objectsToHide;
    [SerializeField] private bool resetVisibilityOnRelease = false;

    [Header("Grab Settings")]
    [SerializeField] private bool snapToHand = true;
    [SerializeField] private bool returnToStartPosOnRelease = false;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool hasTriggeredTimeline = false;

    protected override void Awake()
    {
        base.Awake();

        // Store initial transform
        startPosition = transform.position;
        startRotation = transform.rotation;

        // Configure grab settings
        movementType = snapToHand ? MovementType.Kinematic : MovementType.VelocityTracking;
        throwOnDetach = !returnToStartPosOnRelease;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        // Handle timeline
        if (playTimelineOnGrab && timelineDirector != null && !hasTriggeredTimeline)
        {
            timelineDirector.time = 0;
            timelineDirector.Play();
            hasTriggeredTimeline = true;
        }

        // Handle object visibility
        if (objectsToShow != null)
        {
            foreach (var obj in objectsToShow)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }

        if (objectsToHide != null)
        {
            foreach (var obj in objectsToHide)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        // Handle timeline reset
        if (resetTimelineOnRelease && timelineDirector != null)
        {
            timelineDirector.Stop();
            timelineDirector.time = 0;
            hasTriggeredTimeline = false;
        }

        // Handle visibility reset
        if (resetVisibilityOnRelease)
        {
            if (objectsToShow != null)
            {
                foreach (var obj in objectsToShow)
                {
                    if (obj != null)
                    {
                        obj.SetActive(false);
                    }
                }
            }

            if (objectsToHide != null)
            {
                foreach (var obj in objectsToHide)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);
                    }
                }
            }
        }

        // Return to start position if configured
        if (returnToStartPosOnRelease)
        {
            transform.position = startPosition;
            transform.rotation = startRotation;
        }
    }
}