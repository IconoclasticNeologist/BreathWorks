using UnityEngine;

public class PhysicsElevatorMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float groundY = 0f;
    public float topY = 10f;
    public float speed = 2f;

    private Rigidbody rb;
    private Vector3 targetPosition;
    private bool shouldMove = false;

    private void Start()
    {
        // Add and configure Rigidbody
        rb = gameObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Configure the Rigidbody
        rb.useGravity = false;
        rb.isKinematic = true; // We'll control the movement
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        targetPosition = transform.position;
    }

    public void GoUp()
    {
        Debug.Log("Elevator going up");
        targetPosition = new Vector3(transform.position.x, topY, transform.position.z);
        shouldMove = true;
    }

    public void GoDown()
    {
        Debug.Log("Elevator going down");
        targetPosition = new Vector3(transform.position.x, groundY, transform.position.z);
        shouldMove = true;
    }

    private void FixedUpdate()
    {
        if (shouldMove)
        {
            Vector3 newPosition = Vector3.MoveTowards(rb.position, targetPosition, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);

            if (Vector3.Distance(rb.position, targetPosition) < 0.01f)
            {
                shouldMove = false;
            }
        }
    }
}