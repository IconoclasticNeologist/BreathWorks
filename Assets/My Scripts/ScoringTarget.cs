using UnityEngine;

namespace MyScripts
{
    [RequireComponent(typeof(Collider))]
    public class ScoringTarget : MonoBehaviour
    {
        [Header("Scoring Settings")]
        [SerializeField] private int pointMultiplier = 1;
        [SerializeField] private bool resetStreakOnMiss = true;

        [Header("Visual Feedback")]
        [SerializeField] private Material hitMaterial;
        [SerializeField] private float resetDelay = 1f;
        [SerializeField] private ParticleSystem hitEffect;

        [Header("Audio")]
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioSource audioSource;

        private Material originalMaterial;
        private MeshRenderer meshRenderer;
        private bool canBeHit = true;

        private void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                originalMaterial = meshRenderer.material;
            }

            if (audioSource == null && hitSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!canBeHit) return;

            // Check if the colliding object is a projectile
            if (collision.gameObject.CompareTag("Projectile"))
            {
                HandleHit();
            }
        }

        private void HandleHit()
        {
            canBeHit = false;

            // Add points
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddPoints(pointMultiplier);
            }

            // Visual feedback
            if (meshRenderer != null && hitMaterial != null)
            {
                meshRenderer.material = hitMaterial;
            }

            // Play particle effect
            if (hitEffect != null)
            {
                hitEffect.Play();
            }

            // Play sound
            if (audioSource != null && hitSound != null)
            {
                audioSource.PlayOneShot(hitSound);
            }

            // Reset after delay
            Invoke(nameof(ResetTarget), resetDelay);
        }

        private void ResetTarget()
        {
            if (meshRenderer != null && originalMaterial != null)
            {
                meshRenderer.material = originalMaterial;
            }
            canBeHit = true;
        }

        private void OnTriggerExit(Collider other)
        {
            // If projectile misses and exits trigger zone
            if (resetStreakOnMiss && other.CompareTag("Projectile"))
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ResetStreak();
                }
            }
        }
    }
}