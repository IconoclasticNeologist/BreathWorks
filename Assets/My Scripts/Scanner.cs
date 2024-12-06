using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using System;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class Scanner : XRGrabInteractable
{
    public enum ScannerType
    {
        Maximizer,
        ZeroGravity,
        Visibility
    }

    [Serializable]
    public class MaximizerSettings
    {
        public Vector3 targetScale = Vector3.one * 2f;
        public float scaleSpeed = 1f;
        public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }

    [Serializable]
    public class ZeroGravitySettings
    {
        public float drag = 0f;
        public bool maintainVelocity = true;
        public bool addUpwardForce = false;
        public float upwardForce = 2f;
    }

    [Serializable]
    public class VisibilitySettings
    {
        public float fadeOutDuration = 0.5f;
        public float fadeInDuration = 0.5f;
    }

    [Serializable]
    public class MaterialSettings
    {
        public bool enableMaterialChange = true;
        public Material[] effectMaterials;
        public float cycleDuration = 1f;
    }

    [Header("Scanner Type")]
    [SerializeField] private ScannerType scannerType;

    [Header("Effect Settings")]
    [SerializeField] private bool hasEffectDuration = true;
    [SerializeField] private float effectDuration = 5f;
    [SerializeField] private MaximizerSettings maximizerSettings = new MaximizerSettings();
    [SerializeField] private ZeroGravitySettings zeroGravitySettings = new ZeroGravitySettings();
    [SerializeField] private VisibilitySettings visibilitySettings = new VisibilitySettings();
    [SerializeField] private MaterialSettings materialSettings = new MaterialSettings();

    [Header("Scanner Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private LineRenderer laserLineRenderer;
    [SerializeField] private float laserDistance = 4f;
    [SerializeField] private float laserWidth = 0.01f;
    [SerializeField] private Color laserColor = Color.red;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip scanningSound;
    [SerializeField] private AudioClip successSound;
    [SerializeField] [Range(0f, 1f)] private float volumeMultiplier = 1f;

    [Header("Additional Settings")]
    [SerializeField] private float scanCooldown = 0.5f;
    [SerializeField] private LayerMask scanLayerMask = -1;

    // Events
    public UnityEvent onScanStart;
    public UnityEvent onScanSuccess;
    public UnityEvent onScanFailed;

    // Private variables
    private AudioSource audioSource;
    private bool isScanning;
    private float scanTriggerTime;
    private float lastScanTime;
    private ScannableObject currentTarget;
    private Coroutine activeEffectCoroutine;
    private Coroutine materialCycleCoroutine;
    private Material[] originalMaterials;
    private bool effectActive;

    protected override void Awake()
    {
        base.Awake();
        SetupComponents();

        // Unity 2021.3 XR Interaction Toolkit settings
        movementType = MovementType.VelocityTracking;
        throwOnDetach = false;
        retainTransformParent = true;

        // Initialize material array if needed
        if (materialSettings.effectMaterials == null || materialSettings.effectMaterials.Length == 0)
        {
            materialSettings.effectMaterials = new Material[1];
        }
    }

    private void SetupComponents()
    {
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;

        // Setup laser
        if (laserLineRenderer != null)
        {
            laserLineRenderer.startWidth = laserWidth;
            laserLineRenderer.endWidth = laserWidth;
            laserLineRenderer.startColor = laserColor;
            laserLineRenderer.endColor = laserColor;
            laserLineRenderer.useWorldSpace = false;
            laserLineRenderer.enabled = false;

            // Ensure the line renderer has a material for Unity 2021.3
            if (laserLineRenderer.material == null)
            {
                laserLineRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            }
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        if (pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound, volumeMultiplier);
        }
        if (animator != null)
        {
            animator.SetBool("Opened", true);
        }

        // Unity 2021.3 haptic feedback
        if (args.interactorObject is XRBaseControllerInteractor controllerInteractor)
        {
            controllerInteractor.SendHapticImpulse(0.5f, 0.1f);
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        if (animator != null)
        {
            animator.SetBool("Opened", false);
        }
    }

    protected override void OnActivated(ActivateEventArgs args)
    {
        base.OnActivated(args);
        StartScanning();
    }

    protected override void OnDeactivated(DeactivateEventArgs args)
    {
        base.OnDeactivated(args);
        StopScanning();
    }

    private void StartScanning()
    {
        isScanning = true;
        scanTriggerTime = Time.time;

        if (laserLineRenderer != null)
        {
            laserLineRenderer.enabled = true;
        }

        if (scanningSound != null)
        {
            audioSource.PlayOneShot(scanningSound, volumeMultiplier);
        }
    }

    private void StopScanning()
    {
        isScanning = false;
        if (laserLineRenderer != null)
        {
            laserLineRenderer.enabled = false;
        }

        // Check for reset condition (long press)
        if (Time.time - scanTriggerTime > 1f && currentTarget != null &&
            (scannerType == ScannerType.Maximizer || scannerType == ScannerType.ZeroGravity))
        {
            ResetCurrentEffect();
        }
    }

    private void Update()
    {
        if (isScanning)
        {
            UpdateLaser();
            CheckForScannableObject();
        }
    }

    private void UpdateLaser()
    {
        if (laserLineRenderer != null)
        {
            laserLineRenderer.SetPosition(0, Vector3.zero);
            laserLineRenderer.SetPosition(1, Vector3.forward * laserDistance);
        }
    }

    private void CheckForScannableObject()
    {
        if (Time.time - lastScanTime < scanCooldown) return;

        Ray ray = new Ray(laserLineRenderer.transform.position, laserLineRenderer.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, laserDistance, scanLayerMask))
        {
            ScannableObject scannableObject = hit.collider.GetComponent<ScannableObject>();
            if (scannableObject != null)
            {
                ProcessScan(scannableObject);
            }
        }
    }

    private void ProcessScan(ScannableObject scannableObject)
    {
        if (scannableObject == currentTarget && effectActive) return;

        onScanStart?.Invoke();

        if (scannableObject.ValidateScan(transform.position))
        {
            // Stop any active effects
            if (activeEffectCoroutine != null)
            {
                StopCoroutine(activeEffectCoroutine);
            }

            currentTarget = scannableObject;
            lastScanTime = Time.time;
            effectActive = true;

            scannableObject.OnScanStart();

            // Apply scanner-specific effects
            ApplyScannerTypeEffect(scannableObject);

            if (successSound != null)
            {
                audioSource.PlayOneShot(successSound, volumeMultiplier);
            }

            onScanSuccess?.Invoke();
            scannableObject.OnScanComplete();
        }
        else
        {
            onScanFailed?.Invoke();
            scannableObject.OnScanFailed();
        }
    }

    private void ApplyScannerTypeEffect(ScannableObject scannableObject)
    {
        switch (scannerType)
        {
            case ScannerType.Maximizer:
                activeEffectCoroutine = StartCoroutine(ApplyMaximizerEffect(scannableObject));
                break;

            case ScannerType.ZeroGravity:
                activeEffectCoroutine = StartCoroutine(ApplyZeroGravityEffect(scannableObject));
                break;

            case ScannerType.Visibility:
                activeEffectCoroutine = StartCoroutine(ApplyVisibilityEffect(scannableObject));
                break;
        }

        // Apply material effect if enabled
        if (materialSettings.enableMaterialChange && materialSettings.effectMaterials.Length > 0)
        {
            if (materialCycleCoroutine != null)
            {
                StopCoroutine(materialCycleCoroutine);
            }
            materialCycleCoroutine = StartCoroutine(CycleMaterials(scannableObject));
        }

        scannableObject.OnEffectStart();
    }

    private IEnumerator ApplyMaximizerEffect(ScannableObject scannableObject)
    {
        float elapsed = 0f;
        Vector3 startScale = scannableObject.transform.localScale;

        while (!hasEffectDuration || elapsed < effectDuration)
        {
            float scaleProgress = maximizerSettings.scaleCurve.Evaluate(
                Mathf.Clamp01(elapsed * maximizerSettings.scaleSpeed));
            scannableObject.transform.localScale = Vector3.Lerp(startScale,
                maximizerSettings.targetScale, scaleProgress);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (hasEffectDuration)
        {
            scannableObject.transform.localScale = scannableObject.OriginalScale;
            FinishEffect(scannableObject);
        }
    }

    private IEnumerator ApplyZeroGravityEffect(ScannableObject scannableObject)
    {
        Rigidbody rb = scannableObject.GetOrAddRigidbody();
        Vector3 currentVelocity = rb.velocity;

        rb.useGravity = false;
        rb.drag = zeroGravitySettings.drag;

        if (zeroGravitySettings.maintainVelocity)
        {
            rb.velocity = currentVelocity;
        }

        if (zeroGravitySettings.addUpwardForce)
        {
            rb.AddForce(Vector3.up * zeroGravitySettings.upwardForce, ForceMode.Impulse);
        }

        if (hasEffectDuration)
        {
            yield return new WaitForSeconds(effectDuration);
            FinishEffect(scannableObject);
        }
    }

    private IEnumerator ApplyVisibilityEffect(ScannableObject scannableObject)
    {
        Renderer[] renderers = scannableObject.GetRenderers();

        // Fade out
        float elapsed = 0f;
        while (elapsed < visibilitySettings.fadeOutDuration)
        {
            float alpha = 1 - (elapsed / visibilitySettings.fadeOutDuration);
            SetRenderersAlpha(renderers, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        scannableObject.SetVisibility(false);

        if (hasEffectDuration)
        {
            yield return new WaitForSeconds(effectDuration);

            // Fade back in
            scannableObject.SetVisibility(true);
            elapsed = 0f;
            while (elapsed < visibilitySettings.fadeInDuration)
            {
                float alpha = elapsed / visibilitySettings.fadeInDuration;
                SetRenderersAlpha(renderers, alpha);
                elapsed += Time.deltaTime;
                yield return null;
            }

            FinishEffect(scannableObject);
        }
    }

    private void SetRenderersAlpha(Renderer[] renderers, float alpha)
    {
        foreach (Renderer renderer in renderers)
        {
            foreach (Material material in renderer.materials)
            {
                if (material.HasProperty("_MainColor"))
                {
                    Color color = material.GetColor("_MainColor");
                    color.a = alpha;
                    material.SetColor("_MainColor", color);
                }
                else if (material.HasProperty("_Color"))
                {
                    Color color = material.GetColor("_Color");
                    color.a = alpha;
                    material.SetColor("_Color", color);
                }
            }
        }
    }

    private IEnumerator CycleMaterials(ScannableObject scannableObject)
    {
        int currentIndex = 0;

        while (effectActive)
        {
            scannableObject.SetMaterials(new Material[] { materialSettings.effectMaterials[currentIndex] });
            currentIndex = (currentIndex + 1) % materialSettings.effectMaterials.Length;
            yield return new WaitForSeconds(materialSettings.cycleDuration);
        }
    }

    private void ResetCurrentEffect()
    {
        if (currentTarget != null)
        {
            if (activeEffectCoroutine != null)
            {
                StopCoroutine(activeEffectCoroutine);
            }
            if (materialCycleCoroutine != null)
            {
                StopCoroutine(materialCycleCoroutine);
            }

            currentTarget.ResetObject();
            currentTarget.OnEffectEnd();
            currentTarget = null;
            effectActive = false;
        }
    }

    private void FinishEffect(ScannableObject scannableObject)
    {
        scannableObject.ResetObject();
        scannableObject.OnEffectEnd();
        currentTarget = null;
        effectActive = false;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        ResetCurrentEffect();
    }
}