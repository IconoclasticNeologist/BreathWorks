using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
public class Keycard : XRGrabInteractable
{
    private Rigidbody rb;

    protected override void Awake()
    {
        base.Awake();

        // Configure the grab interaction
        selectMode = InteractableSelectMode.Single;

        // Get reference to rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Make sure rigidbody is kinematic to avoid the non-convex mesh collider error
            rb.isKinematic = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    public float GetVelocityMagnitude()
    {
        if (rb != null)
        {
            return rb.velocity.magnitude;
        }
        return 0f;
    }
}