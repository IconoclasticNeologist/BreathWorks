using UnityEngine;
using TMPro;
using System;

public class NumberPad : MonoBehaviour
{
    [Serializable]
    public class SpawnableObject
    {
        public string requiredCode;
        public GameObject prefabToSpawn;
        public string successMessage = "OK";
        public bool useSpawnPoint = true;
    }

    [Header("Button Settings")]
    [SerializeField] private int resetButtonNumber = 0;
    [SerializeField] private int deleteButtonNumber = -1;
    [SerializeField] private float buttonCooldown = 1f;
    [SerializeField] private int codeLength = 4;

    [Header("Spawnable Objects")]
    [SerializeField] private SpawnableObject[] spawnableObjects;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnDistance = 0.3f;
    [SerializeField] private float spawnDuration = 1.0f;

    [Header("UI and Audio")]
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private GameObject accessGrantedScreen;
    [SerializeField] private NumberpadScreenDispenser screenDispenser;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip incorrectSound;
    [SerializeField] private AudioClip resetSound;
    [SerializeField] private AudioClip deleteSound;

    private string currentEnteredCode = "";
    private bool canEnterCode = true;
    private float lastPressTime = -999f;

    private void Start()
    {
        if (accessGrantedScreen != null)
        {
            accessGrantedScreen.SetActive(false);
        }

        // Try to find screen dispenser if not assigned
        if (screenDispenser == null)
        {
            screenDispenser = FindObjectOfType<NumberpadScreenDispenser>();
        }

        // Validate configuration
        foreach (var obj in spawnableObjects)
        {
            if (obj.requiredCode.Length != codeLength)
            {
                Debug.LogWarning($"Code {obj.requiredCode} does not match the expected length of {codeLength} digits!");
            }
        }
    }

    public void ButtonPressed(int valuePressed)
    {
        // Check cooldown
        if (Time.time - lastPressTime < buttonCooldown)
        {
            return;
        }

        // Update last press time
        lastPressTime = Time.time;

        // Handle special buttons
        if (valuePressed == resetButtonNumber)
        {
            HandleReset();
            return;
        }

        if (valuePressed == deleteButtonNumber)
        {
            HandleDelete();
            return;
        }

        // Handle number input
        if (canEnterCode && currentEnteredCode.Length < codeLength)
        {
            currentEnteredCode += valuePressed.ToString();
            displayText.text = currentEnteredCode;

            if (currentEnteredCode.Length == codeLength)
            {
                CheckCode();
            }
        }
    }

    private void HandleDelete()
    {
        if (currentEnteredCode.Length > 0 && canEnterCode)
        {
            // Remove the last character
            currentEnteredCode = currentEnteredCode.Substring(0, currentEnteredCode.Length - 1);
            displayText.text = currentEnteredCode;

            // Play delete sound if assigned
            if (audioSource != null && deleteSound != null)
            {
                audioSource.PlayOneShot(deleteSound);
            }
        }
    }

    private void HandleReset()
    {
        if (audioSource != null && resetSound != null)
        {
            audioSource.PlayOneShot(resetSound);
        }

        canEnterCode = true;
        ResetDisplay();
        displayText.text = "READY";

        if (accessGrantedScreen != null)
        {
            accessGrantedScreen.SetActive(false);
        }

        // Hide just the screen display
        if (screenDispenser != null)
        {
            screenDispenser.HideScreen();
        }

        Invoke(nameof(ClearReadyMessage), 1.0f);
    }

    private void ClearReadyMessage()
    {
        if (displayText.text == "READY")
        {
            displayText.text = "";
        }
    }

    private void CheckCode()
    {
        bool codeFound = false;

        foreach (var spawnableObject in spawnableObjects)
        {
            if (currentEnteredCode == spawnableObject.requiredCode && spawnableObject.prefabToSpawn != null)
            {
                HandleCorrectCode(spawnableObject.successMessage);
                ShowAccessGranted();
                SpawnObject(spawnableObject);
                codeFound = true;
                break;
            }
        }

        if (!codeFound)
        {
            if (audioSource != null && incorrectSound != null)
                audioSource.PlayOneShot(incorrectSound);

            displayText.text = "ERROR";
            displayText.color = Color.red;
            Invoke(nameof(ResetDisplay), 1.5f);
        }
    }

    private void ShowAccessGranted()
    {
        if (accessGrantedScreen != null)
        {
            accessGrantedScreen.SetActive(true);
        }

        // Trigger screen dispenser
        if (screenDispenser != null)
        {
            screenDispenser.DisplayScreen();
        }
        else
        {
            // Try one more time to find the screen dispenser
            screenDispenser = FindObjectOfType<NumberpadScreenDispenser>();
            if (screenDispenser != null)
            {
                screenDispenser.DisplayScreen();
            }
        }
    }

    private void HandleCorrectCode(string message)
    {
        if (audioSource != null && correctSound != null)
            audioSource.PlayOneShot(correctSound);

        displayText.text = message;
        displayText.color = Color.green;
        canEnterCode = false;
    }

    private void SpawnObject(SpawnableObject objectToSpawn)
    {
        if (objectToSpawn.prefabToSpawn != null)
        {
            Vector3 spawnPosition = objectToSpawn.useSpawnPoint && spawnPoint != null ?
                spawnPoint.position : transform.position;
            Quaternion spawnRotation = objectToSpawn.useSpawnPoint && spawnPoint != null ?
                spawnPoint.rotation : transform.rotation;

            GameObject spawnedObject = Instantiate(objectToSpawn.prefabToSpawn, spawnPosition, spawnRotation);
            StartCoroutine(AnimateSpawn(spawnedObject.transform));
        }
    }

    private System.Collections.IEnumerator AnimateSpawn(Transform objectTransform)
    {
        Vector3 startPos = objectTransform.position;
        Vector3 endPos = startPos + (spawnPoint != null ? spawnPoint.forward : transform.forward) * spawnDistance;
        float elapsedTime = 0;

        while (elapsedTime < spawnDuration)
        {
            elapsedTime += Time.deltaTime;
            float percentComplete = elapsedTime / spawnDuration;
            float smoothPercent = percentComplete * percentComplete * (3 - 2 * percentComplete);
            objectTransform.position = Vector3.Lerp(startPos, endPos, smoothPercent);
            yield return null;
        }

        objectTransform.position = endPos;

        Rigidbody rb = objectTransform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }

    private void ResetDisplay()
    {
        currentEnteredCode = "";
        displayText.text = "";
        displayText.color = Color.black;
    }
}