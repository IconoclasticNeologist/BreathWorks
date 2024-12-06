using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DoorPadlock : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject padlock;
    [SerializeField] private GameObject padlockRing;
    [SerializeField] private GameObject doorLockingBar;
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private DoorHandle doorHandle;

    private AudioSource audioSource;
    private bool isUnlocked = false;

    private void Start()
    {
        // Add audio source if we have unlock sound
        if (unlockSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Initially disable door handle interaction
        if (doorHandle != null)
        {
            doorHandle.SetHandleEnabled(false);
        }
    }

    public void Unlock()
    {
        if (isUnlocked) return;

        isUnlocked = true;

        // Play unlock sound
        if (audioSource != null && unlockSound != null)
        {
            audioSource.PlayOneShot(unlockSound);
        }

        // Hide padlock and ring
        if (padlock != null) padlock.SetActive(false);
        if (padlockRing != null) padlockRing.SetActive(false);

        // Enable door handle interaction
        if (doorHandle != null)
        {
            doorHandle.SetHandleEnabled(true);
        }

        // Make locking bar fall if it has physics
        if (doorLockingBar != null)
        {
            Rigidbody rb = doorLockingBar.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }
        }
    }
}