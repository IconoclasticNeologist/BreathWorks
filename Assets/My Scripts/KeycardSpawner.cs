using UnityEngine;
using System.Collections;

public class KeycardSpawner : MonoBehaviour
{
    [SerializeField] private GameObject keycardPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float dispenseDuration = 1.0f; // How long the animation takes
    [SerializeField] private float dispenseDistance = 0.2f; // How far the card moves
    [SerializeField] private AudioClip dispenseSound; // Optional: Sound effect for dispensing
    private AudioSource audioSource;

    private void Start()
    {
        // Add AudioSource if we have a dispense sound
        if (dispenseSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public void SpawnCard()
    {
        StartCoroutine(DispenseCardAnimation());
    }

    private IEnumerator DispenseCardAnimation()
    {
        // Play dispense sound if we have one
        if (audioSource != null && dispenseSound != null)
        {
            audioSource.PlayOneShot(dispenseSound);
        }

        // Spawn the card at the start position
        GameObject card = Instantiate(keycardPrefab, spawnPoint.position, spawnPoint.rotation);

        // Calculate end position
        Vector3 startPos = spawnPoint.position;
        Vector3 endPos = startPos + (spawnPoint.forward * dispenseDistance);

        float elapsedTime = 0;

        // Animate the card moving out
        while (elapsedTime < dispenseDuration)
        {
            elapsedTime += Time.deltaTime;
            float percentageComplete = elapsedTime / dispenseDuration;

            // Use easeOut curve for smoother motion
            float smoothPercentage = 1 - Mathf.Pow(1 - percentageComplete, 2);

            // Move the card
            card.transform.position = Vector3.Lerp(startPos, endPos, smoothPercentage);

            yield return null;
        }

        // Ensure card ends up exactly at end position
        card.transform.position = endPos;

        // Enable physics after animation if the card has a Rigidbody
        Rigidbody rb = card.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;
        }
    }
}