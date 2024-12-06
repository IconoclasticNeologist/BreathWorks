using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Playables;

public class HandleTimelineTrigger : MonoBehaviour
{
    [SerializeField] private PlayableDirector doorTimeline;
    [SerializeField] private bool canTriggerMultipleTimes = false;

    private XRGrabInteractable grabInteractable;
    private bool hasBeenTriggered = false;

    private void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnHandleGrabbed);
        }
    }

    private void OnHandleGrabbed(SelectEnterEventArgs args)
    {
        if (doorTimeline != null && (canTriggerMultipleTimes || !hasBeenTriggered))
        {
            doorTimeline.time = 0;
            doorTimeline.Play();
            hasBeenTriggered = true;
            Debug.Log("Timeline triggered by handle grab");
        }
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnHandleGrabbed);
        }
    }
}