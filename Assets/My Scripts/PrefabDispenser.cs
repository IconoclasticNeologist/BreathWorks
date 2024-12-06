using UnityEngine;
using System.Collections;

public class PrefabDispenser : MonoBehaviour
{
    [System.Serializable]
    public class PrefabData
    {
        public string buttonName;
        public GameObject prefab;
        public Transform spawnPoint;
    }

    [Header("Dispenser Settings")]
    [SerializeField] private PrefabData[] prefabMappings;
    [SerializeField] private float dispenseDuration = 1.0f;
    [SerializeField] private float dispenseDistance = 0.2f;

    [Header("Audio")]
    [SerializeField] private AudioClip dispenseSound;
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

    public void SpawnPrefab(string buttonName)
    {
        // Find the matching prefab data
        PrefabData matchingData = System.Array.Find(prefabMappings, data => data.buttonName == buttonName);

        if (matchingData != null && matchingData.prefab != null)
        {
            StartCoroutine(DispenseAnimation(matchingData));
        }
        else
        {
            Debug.LogWarning($"No prefab mapping found for button: {buttonName}");
        }
    }

    private IEnumerator DispenseAnimation(PrefabData data)
    {
        // Play dispense sound if we have one
        if (audioSource != null && dispenseSound != null)
        {
            audioSource.PlayOneShot(dispenseSound);
        }

        // Use the specified spawn point or fall back to this object's position
        Vector3 spawnPosition = data.spawnPoint != null ? data.spawnPoint.position : transform.position;
        Quaternion spawnRotation = data.spawnPoint != null ? data.spawnPoint.rotation : transform.rotation;

        // Spawn the prefab
        GameObject dispensedObject = Instantiate(data.prefab, spawnPosition, spawnRotation);

        // Calculate end position
        Vector3 moveDirection = data.spawnPoint != null ? data.spawnPoint.forward : transform.forward;
        Vector3 endPos = spawnPosition + (moveDirection * dispenseDistance);

        float elapsedTime = 0;

        // Animate the object moving out
        while (elapsedTime < dispenseDuration)
        {
            elapsedTime += Time.deltaTime;
            float percentageComplete = elapsedTime / dispenseDuration;

            // Use easeOut curve for smoother motion
            float smoothPercentage = 1 - Mathf.Pow(1 - percentageComplete, 2);

            // Move the object
            dispensedObject.transform.position = Vector3.Lerp(spawnPosition, endPos, smoothPercentage);

            yield return null;
        }

        // Ensure object ends up exactly at end position
        dispensedObject.transform.position = endPos;

        // Enable physics after animation
        Rigidbody rb = dispensedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;
        }
    }
}