using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AdvancedPrefabDispenser : MonoBehaviour
{
    [System.Serializable]
    public class InteractionSettings
    {
        [Header("Components")]
        public bool addGrabTrigger = true;

        [Header("Grab Settings")]
        public bool snapToHand = true;
    }

    [System.Serializable]
    public class PrefabData
    {
        public string buttonName;
        public GameObject prefab;
        public Transform spawnPoint;
        public InteractionSettings interactionSettings;

        [Header("Spawn Settings")]
        [Min(0)] public float cooldownTime = 0f;
        [Min(0)] public int maxSpawns = -1; // -1 for unlimited

        [Header("Effects")]
        public bool useCustomParticles = false;
        public ParticleSystem spawnParticles;
        public AudioClip spawnSound;
        [Range(0.1f, 2f)] public float soundPitchVariation = 1f;
    }

    [Header("Prefab Configuration")]
    [SerializeField] private PrefabData[] prefabMappings;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDuration = 1.0f;
    [SerializeField] private float spawnDistance = 0.2f;
    [SerializeField] private AnimationCurve spawnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Audio")]
    [SerializeField] private AudioClip defaultSpawnSound;
    [SerializeField] [Range(0f, 1f)] private float volume = 1f;

    [Header("Cleanup")]
    [SerializeField] private bool autoCleanup = true;
    [SerializeField] private float cleanupDelay = 30f;
    [SerializeField] private int maxSpawnedObjects = 50;

    private AudioSource audioSource;
    private Dictionary<string, float> cooldownTimers = new Dictionary<string, float>();
    private Dictionary<string, int> spawnCounts = new Dictionary<string, int>();
    private List<GameObject> spawnedObjects = new List<GameObject>();

    private void Awake()
    {
        SetupAudio();
        SetupCooldowns();
    }

    private void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void SetupCooldowns()
    {
        cooldownTimers.Clear();
        spawnCounts.Clear();
        foreach (var data in prefabMappings)
        {
            cooldownTimers[data.buttonName] = 0f;
            spawnCounts[data.buttonName] = 0;
        }
    }

    private void Start()
    {
        if (autoCleanup)
        {
            StartCoroutine(HandleCleanup());
        }
    }

    private void Update()
    {
        HandleCooldowns();
    }

    private void HandleCooldowns()
    {
        List<string> buttons = new List<string>(cooldownTimers.Keys);
        foreach (var button in buttons)
        {
            if (cooldownTimers[button] > 0)
            {
                cooldownTimers[button] -= Time.deltaTime;
            }
        }
    }

    public void SpawnPrefab(string buttonName)
    {
        PrefabData data = System.Array.Find(prefabMappings, d => d.buttonName == buttonName);

        if (!CanSpawn(buttonName, data))
        {
            return;
        }

        StartCoroutine(SpawnRoutine(data));
    }

    private bool CanSpawn(string buttonName, PrefabData data)
    {
        if (data == null || data.prefab == null) return false;
        if (cooldownTimers.ContainsKey(buttonName) && cooldownTimers[buttonName] > 0) return false;
        if (data.maxSpawns > 0 && spawnCounts[buttonName] >= data.maxSpawns) return false;
        if (maxSpawnedObjects > 0 && spawnedObjects.Count >= maxSpawnedObjects) return false;
        return true;
    }

    private IEnumerator SpawnRoutine(PrefabData data)
    {
        cooldownTimers[data.buttonName] = data.cooldownTime;
        spawnCounts[data.buttonName]++;

        Vector3 spawnPosition = data.spawnPoint != null ? data.spawnPoint.position : transform.position;
        Quaternion spawnRotation = data.spawnPoint != null ? data.spawnPoint.rotation : transform.rotation;

        GameObject spawnedObject = CreateSpawnedObject(data, spawnPosition, spawnRotation);
        spawnedObjects.Add(spawnedObject);

        HandleSpawnSound(data);

        Vector3 moveDirection = data.spawnPoint != null ? data.spawnPoint.forward : transform.forward;
        Vector3 endPos = spawnPosition + (moveDirection * spawnDistance);
        float elapsedTime = 0;

        // Keep object kinematic during spawn animation
        Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        while (elapsedTime < spawnDuration)
        {
            elapsedTime += Time.deltaTime;
            float percentComplete = elapsedTime / spawnDuration;
            float curveValue = spawnCurve.Evaluate(percentComplete);

            spawnedObject.transform.position = Vector3.Lerp(spawnPosition, endPos, curveValue);
            yield return null;
        }

        spawnedObject.transform.position = endPos;
    }

    private GameObject CreateSpawnedObject(PrefabData data, Vector3 position, Quaternion rotation)
    {
        GameObject spawnedObject = Instantiate(data.prefab, position, rotation);

        if (data.interactionSettings.addGrabTrigger)
        {
            XRGrabInteractable grabInteractable = spawnedObject.AddComponent<XRGrabInteractable>();

            grabInteractable.movementType = XRBaseInteractable.MovementType.Kinematic;
            grabInteractable.throwOnDetach = false;
            grabInteractable.trackPosition = true;
            grabInteractable.trackRotation = true;
            grabInteractable.smoothPosition = true;
            grabInteractable.smoothRotation = true;

            Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = spawnedObject.AddComponent<Rigidbody>();
            }
            rb.useGravity = false;
            rb.isKinematic = true;

            if (spawnedObject.GetComponent<Collider>() == null)
            {
                spawnedObject.AddComponent<BoxCollider>();
            }
        }

        return spawnedObject;
    }

    private void HandleSpawnSound(PrefabData data)
    {
        if (audioSource != null)
        {
            AudioClip clipToPlay = data.spawnSound != null ? data.spawnSound : defaultSpawnSound;
            if (clipToPlay != null)
            {
                float pitch = 1f;
                if (data.soundPitchVariation != 1f)
                {
                    pitch *= Random.Range(1f / data.soundPitchVariation, data.soundPitchVariation);
                }

                audioSource.pitch = pitch;
                audioSource.PlayOneShot(clipToPlay, volume);
            }
        }
    }

    private IEnumerator HandleCleanup()
    {
        while (true)
        {
            yield return new WaitForSeconds(cleanupDelay);

            while (spawnedObjects.Count > maxSpawnedObjects)
            {
                if (spawnedObjects[0] != null)
                {
                    Destroy(spawnedObjects[0]);
                }
                spawnedObjects.RemoveAt(0);
            }
        }
    }
}