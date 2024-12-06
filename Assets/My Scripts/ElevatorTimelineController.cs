using UnityEngine;
using UnityEngine.Playables;

public class ElevatorTimelineController : MonoBehaviour
{
    private PlayableDirector playableDirector;

    private void Awake()
    {
        playableDirector = GetComponent<PlayableDirector>();
        if (playableDirector == null)
        {
            Debug.LogError("No PlayableDirector found on ElevatorTimeline object!");
        }
    }

    public void PlayTimeline()
    {
        if (playableDirector != null)
        {
            playableDirector.time = 0;  // Reset to start
            playableDirector.Play();
            Debug.Log("Playing elevator timeline");
        }
    }

    public void StopTimeline()
    {
        if (playableDirector != null)
        {
            playableDirector.Stop();
        }
    }
}