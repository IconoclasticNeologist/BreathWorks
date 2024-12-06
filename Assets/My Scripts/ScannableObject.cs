using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

[AddComponentMenu("Scanner/Scannable Object")]
public class ScannableObject : MonoBehaviour
{
    [Serializable]
    public class ScanLimitations
    {
        public bool enableScanLimits = false;
        [Min(0)] public int maxScanCount = 3;
        [Min(0)] public float scanCooldown = 2f;
        [Min(0)] public float maxScanRange = 5f;
        public string scanCategory = "Default";
    }

    [Header("Core Settings")]
    [SerializeField] private bool startActive = true;
    [SerializeField] private bool resetOnDisable = true;

    [Header("Scan Limitations")]
    [SerializeField] private ScanLimitations scanLimitations;

    [Header("Events")]
    public UnityEvent onScanStart;
    public UnityEvent onScanComplete;
    public UnityEvent onScanFailed;
    public UnityEvent onEffectStart;
    public UnityEvent onEffectEnd;
    public UnityEvent onReset;

    // Component references
    private Renderer[] renderers;
    private Dictionary<Renderer, Material[]> originalMaterialsMap;
    private Vector3 originalScale;
    private Rigidbody rb;

    // State tracking
    private int currentScanCount = 0;
    private float lastScanTime = -999f;
    private bool canScan = true;
    private bool isBeingScanned = false;

    private void Awake()
    {
        Initialize();
        // Enable/disable scanning based on startActive
        canScan = startActive;
        foreach (var renderer in renderers)
        {
            if (renderer != null)
                renderer.enabled = startActive;
        }
    }

    private void Initialize()
    {
        // Cache components
        renderers = GetComponentsInChildren<Renderer>();
        rb = GetComponent<Rigidbody>();

        // Store original states
        StoreOriginalStates();

        // Set tag
        gameObject.tag = "Scannable";
    }

    private void StoreOriginalStates()
    {
        // Store scale
        originalScale = transform.localScale;

        // Store materials
        originalMaterialsMap = new Dictionary<Renderer, Material[]>();
        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                originalMaterialsMap[renderer] = renderer.sharedMaterials;
            }
        }
    }

    public bool ValidateScan(Vector3 scannerPosition)
    {
        if (!canScan || isBeingScanned || !startActive) return false;

        if (scanLimitations.enableScanLimits)
        {
            // Check scan count
            if (scanLimitations.maxScanCount > 0 && currentScanCount >= scanLimitations.maxScanCount)
                return false;

            // Check cooldown
            if (Time.time - lastScanTime < scanLimitations.scanCooldown)
                return false;

            // Check range
            if (scanLimitations.maxScanRange > 0 &&
                Vector3.Distance(transform.position, scannerPosition) > scanLimitations.maxScanRange)
                return false;
        }

        return true;
    }

    public void OnScanStart()
    {
        if (this == null || !gameObject.activeInHierarchy || !startActive) return;

        isBeingScanned = true;
        currentScanCount++;
        lastScanTime = Time.time;
        onScanStart?.Invoke();
    }

    public void OnScanComplete()
    {
        if (this == null || !gameObject.activeInHierarchy || !startActive) return;

        onScanComplete?.Invoke();
    }

    public void OnScanFailed()
    {
        if (this == null || !gameObject.activeInHierarchy || !startActive) return;

        onScanFailed?.Invoke();
    }

    public void OnEffectStart()
    {
        if (this == null || !gameObject.activeInHierarchy || !startActive) return;

        onEffectStart?.Invoke();
    }

    public void OnEffectEnd()
    {
        if (this == null || !gameObject.activeInHierarchy) return;

        isBeingScanned = false;
        onEffectEnd?.Invoke();
    }

    // Material handling
    public void SetMaterials(Material[] materials)
    {
        if (materials == null || renderers == null) return;

        foreach (var renderer in renderers)
        {
            if (renderer == null) continue;

            Material[] newMaterials = new Material[renderer.materials.Length];
            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = materials[Mathf.Min(i, materials.Length - 1)];
            }
            renderer.materials = newMaterials;
        }
    }

    public void RestoreOriginalMaterials()
    {
        if (renderers == null || originalMaterialsMap == null) return;

        foreach (var renderer in renderers)
        {
            if (renderer == null) continue;
            if (originalMaterialsMap.ContainsKey(renderer))
            {
                renderer.materials = originalMaterialsMap[renderer];
            }
        }
    }

    // Visibility handling
    public void SetVisibility(bool visible)
    {
        if (renderers == null) return;

        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = visible && startActive;
            }
        }
    }

    public Renderer[] GetRenderers()
    {
        return renderers;
    }

    // Rigidbody handling
    public Rigidbody GetOrAddRigidbody()
    {
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        return rb;
    }

    public void ResetObject()
    {
        if (this == null || !gameObject.activeInHierarchy) return;

        // Reset transform
        transform.localScale = originalScale;

        // Reset materials
        RestoreOriginalMaterials();

        // Reset rigidbody if exists
        if (rb != null)
        {
            rb.useGravity = true;
            rb.drag = 0.05f; // Default Unity value
        }

        // Reset scan states
        currentScanCount = 0;
        lastScanTime = -999f;
        canScan = startActive;
        isBeingScanned = false;

        // Ensure object is visible
        SetVisibility(startActive);

        onReset?.Invoke();
    }

    private void OnDisable()
    {
        if (resetOnDisable)
        {
            ResetObject();
        }
    }

    // Public setters
    public void SetStartActive(bool active)
    {
        startActive = active;
        canScan = active;
        SetVisibility(active);
    }

    // Public getters for external scripts
    public bool CanScan => canScan && startActive;
    public bool IsBeingScanned => isBeingScanned;
    public int RemainingScans => scanLimitations.enableScanLimits ?
        Mathf.Max(0, scanLimitations.maxScanCount - currentScanCount) : -1;
    public float CooldownRemaining => scanLimitations.enableScanLimits ?
        Mathf.Max(0, scanLimitations.scanCooldown - (Time.time - lastScanTime)) : 0;
    public string ScanCategory => scanLimitations.scanCategory;
    public Vector3 OriginalScale => originalScale;
}