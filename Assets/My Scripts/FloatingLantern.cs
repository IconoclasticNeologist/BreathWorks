using UnityEngine;
using Unity.XRContent.Interaction;

public class FloatingLantern : MonoBehaviour
{
    [Header("Float Settings")]
    [SerializeField] private float floatForce = 2f;
    [SerializeField] private float maxVelocity = 3f;
    [SerializeField] private float damping = 0.5f;
    [SerializeField] private float randomForce = 0.1f;

    [Header("Trigger Options")]
    [SerializeField] private bool floatOnStart;
    [SerializeField] private bool floatOnPlayerProximity;
    [SerializeField] private float proximityDistance = 2f;
    [SerializeField] private OnTrigger lightAreaTrigger;  // Reference to the LightArea's OnTrigger component

    private Rigidbody rb;
    private bool isFloating;
    private Transform playerCamera;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        playerCamera = Camera.main?.transform;

        if (floatOnStart) StartFloating();

        // Subscribe to the LightArea's OnTrigger events
        if (lightAreaTrigger != null)
        {
            lightAreaTrigger.OnEnter.AddListener(OnLightAreaTriggered);
        }
        else
        {
            // Try to find the LightArea trigger in children if not assigned
            var lightArea = GetComponentInChildren<OnTrigger>();
            if (lightArea != null)
            {
                lightAreaTrigger = lightArea;
                lightAreaTrigger.OnEnter.AddListener(OnLightAreaTriggered);
            }
        }
    }

    private void OnLightAreaTriggered(GameObject trigger)
    {
        // Check if the trigger has the correct tag (probably "Flame" based on your screenshot)
        if (trigger.CompareTag("Flame"))
        {
            StartFloating();
        }
    }

    private void Update()
    {
        if (floatOnPlayerProximity && !isFloating && playerCamera != null)
        {
            float distance = Vector3.Distance(transform.position, playerCamera.position);
            if (distance < proximityDistance)
            {
                StartFloating();
            }
        }
    }

    private void FixedUpdate()
    {
        if (isFloating)
        {
            Vector3 force = Vector3.up * floatForce;
            force += new Vector3(
                Random.Range(-randomForce, randomForce),
                0,
                Random.Range(-randomForce, randomForce)
            );

            rb.velocity = Vector3.ClampMagnitude(
                Vector3.Lerp(rb.velocity, force, Time.fixedDeltaTime),
                maxVelocity
            );
        }
    }

    public void StartFloating()
    {
        if (!isFloating)
        {
            rb.isKinematic = false;
            isFloating = true;
        }
    }

    private void OnDestroy()
    {
        if (lightAreaTrigger != null)
        {
            lightAreaTrigger.OnEnter.RemoveListener(OnLightAreaTriggered);
        }
    }
}