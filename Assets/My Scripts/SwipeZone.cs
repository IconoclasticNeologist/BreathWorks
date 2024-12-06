using UnityEngine;

public class SwipeZone : MonoBehaviour
{
    private CardReader cardReader;

    private void Start()
    {
        // Get reference to the parent CardReader
        cardReader = GetComponentInParent<CardReader>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (cardReader != null)
        {
            Keycard keycard = other.GetComponent<Keycard>();
            if (keycard != null)
            {
                cardReader.HandleCardSwipe();
            }
        }
    }
}