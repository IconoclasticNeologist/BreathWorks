using UnityEngine;
using UnityEngine.Events;

public class SimpleButtonScanner : MonoBehaviour
{
    public UnityEvent onScanned;

    private void Start()
    {
        if (!gameObject.CompareTag("Scannable"))
        {
            gameObject.tag = "Scannable";
        }
    }

    public void OnScanned()
    {
        Debug.Log($"Button {gameObject.name} scanned!");
        onScanned.Invoke();
    }
}