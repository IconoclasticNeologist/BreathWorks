using UnityEngine;
using System;
using UnityEngine.Playables;
using System.Collections.Generic;

namespace MyScripts
{
    [Serializable]
    public class StreakEffectSettings
    {
        [Tooltip("The mesh effect prefab from MESH EFFECTS asset")]
        public GameObject meshEffect;

        [Tooltip("Optional particle system to play")]
        public ParticleSystem particleEffect;

        [Tooltip("Sound to play when this streak is reached")]
        public AudioClip streakSound;

        [Tooltip("How long the effects should last")]
        [Range(0.1f, 5f)]
        public float effectDuration = 1f;

        [Tooltip("Optional timeline to play")]
        public PlayableAsset timelineAsset;

        [Tooltip("Check to use default settings if no effects are set")]
        public bool useDefaultSettingsIfEmpty = true;

        [Tooltip("Volume for the streak sound")]
        [Range(0f, 1f)]
        public float soundVolume = 1f;
    }

    public class StreakEffectsManager : MonoBehaviour
    {
        [Header("Performance Settings")]
        [SerializeField] private bool useObjectPool = true;
        [SerializeField] private int poolSize = 5;
        [Range(1, 10)]
        [SerializeField] private int maxSimultaneousEffects = 3;

        [Header("Streak Effect Settings")]
        [SerializeField] private StreakEffectSettings[] streakLevels = new StreakEffectSettings[10];

        [Header("Default Settings (For Streaks > 10)")]
        [SerializeField] private StreakEffectSettings defaultSettings;

        [Header("Component References")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private PlayableDirector playableDirector;
        [SerializeField] private Transform effectSpawnPoint;

        // Object pooling
        private Dictionary<GameObject, Queue<GameObject>> effectPools;
        private Dictionary<ParticleSystem, Queue<ParticleSystem>> particlePools;
        private List<GameObject> activeEffects;
        private List<ParticleSystem> activeParticles;

        private void Awake()
        {
            InitializeComponents();
            if (useObjectPool)
            {
                InitializeObjectPools();
            }
        }

        private void InitializeComponents()
        {
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            if (effectSpawnPoint == null)
                effectSpawnPoint = transform;

            activeEffects = new List<GameObject>();
            activeParticles = new List<ParticleSystem>();
        }

        private void InitializeObjectPools()
        {
            effectPools = new Dictionary<GameObject, Queue<GameObject>>();
            particlePools = new Dictionary<ParticleSystem, Queue<ParticleSystem>>();

            // Pool mesh effects
            foreach (var settings in streakLevels)
            {
                if (settings?.meshEffect != null && !effectPools.ContainsKey(settings.meshEffect))
                {
                    var queue = new Queue<GameObject>();
                    for (int i = 0; i < poolSize; i++)
                    {
                        var obj = CreatePooledEffect(settings.meshEffect);
                        queue.Enqueue(obj);
                    }
                    effectPools[settings.meshEffect] = queue;
                }

                if (settings?.particleEffect != null && !particlePools.ContainsKey(settings.particleEffect))
                {
                    var queue = new Queue<ParticleSystem>();
                    for (int i = 0; i < poolSize; i++)
                    {
                        var obj = CreatePooledParticle(settings.particleEffect);
                        queue.Enqueue(obj);
                    }
                    particlePools[settings.particleEffect] = queue;
                }
            }

            // Pool default effects
            if (defaultSettings != null)
            {
                if (defaultSettings.meshEffect != null && !effectPools.ContainsKey(defaultSettings.meshEffect))
                {
                    var queue = new Queue<GameObject>();
                    for (int i = 0; i < poolSize; i++)
                    {
                        var obj = CreatePooledEffect(defaultSettings.meshEffect);
                        queue.Enqueue(obj);
                    }
                    effectPools[defaultSettings.meshEffect] = queue;
                }

                if (defaultSettings.particleEffect != null && !particlePools.ContainsKey(defaultSettings.particleEffect))
                {
                    var queue = new Queue<ParticleSystem>();
                    for (int i = 0; i < poolSize; i++)
                    {
                        var obj = CreatePooledParticle(defaultSettings.particleEffect);
                        queue.Enqueue(obj);
                    }
                    particlePools[defaultSettings.particleEffect] = queue;
                }
            }
        }

        private GameObject CreatePooledEffect(GameObject prefab)
        {
            var obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            return obj;
        }

        private ParticleSystem CreatePooledParticle(ParticleSystem prefab)
        {
            var obj = Instantiate(prefab, transform);
            obj.gameObject.SetActive(false);
            return obj;
        }

        public void HandleStreak(int streakCount)
        {
            StreakEffectSettings settings;

            if (streakCount <= 10 && streakCount > 0)
            {
                settings = streakLevels[streakCount - 1];
                if (settings.useDefaultSettingsIfEmpty && IsSettingsEmpty(settings))
                {
                    settings = defaultSettings;
                }
            }
            else
            {
                settings = defaultSettings;
            }

            if (settings == null) return;

            // Limit simultaneous effects
            while (activeEffects.Count >= maxSimultaneousEffects)
            {
                ReturnOldestEffectToPool();
            }

            PlayEffects(settings);
        }

        private bool IsSettingsEmpty(StreakEffectSettings settings)
        {
            return settings.meshEffect == null &&
                   settings.particleEffect == null &&
                   settings.streakSound == null &&
                   settings.timelineAsset == null;
        }

        private void PlayEffects(StreakEffectSettings settings)
        {
            // Spawn mesh effect
            if (settings.meshEffect != null)
            {
                GameObject effect;
                if (useObjectPool && effectPools.ContainsKey(settings.meshEffect))
                {
                    effect = GetPooledEffect(settings.meshEffect);
                }
                else
                {
                    effect = Instantiate(settings.meshEffect, effectSpawnPoint);
                }

                if (effect != null)
                {
                    effect.transform.position = effectSpawnPoint.position;
                    effect.transform.rotation = effectSpawnPoint.rotation;
                    effect.SetActive(true);
                    activeEffects.Add(effect);
                    StartCoroutine(DeactivateAfterDelay(effect, settings.effectDuration));
                }
            }

            // Spawn particle effect
            if (settings.particleEffect != null)
            {
                ParticleSystem particleSystem;
                if (useObjectPool && particlePools.ContainsKey(settings.particleEffect))
                {
                    particleSystem = GetPooledParticle(settings.particleEffect);
                }
                else
                {
                    particleSystem = Instantiate(settings.particleEffect, effectSpawnPoint);
                }

                if (particleSystem != null)
                {
                    particleSystem.transform.position = effectSpawnPoint.position;
                    particleSystem.transform.rotation = effectSpawnPoint.rotation;
                    particleSystem.gameObject.SetActive(true);
                    particleSystem.Play();
                    activeParticles.Add(particleSystem);
                    StartCoroutine(DeactivateParticleAfterDelay(particleSystem, settings.effectDuration));
                }
            }

            // Play sound
            if (settings.streakSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(settings.streakSound, settings.soundVolume);
            }

            // Play timeline
            if (settings.timelineAsset != null && playableDirector != null)
            {
                playableDirector.playableAsset = settings.timelineAsset;
                playableDirector.Play();
            }
        }

        private GameObject GetPooledEffect(GameObject prefab)
        {
            if (effectPools[prefab].Count > 0)
                return effectPools[prefab].Dequeue();

            return CreatePooledEffect(prefab);
        }

        private ParticleSystem GetPooledParticle(ParticleSystem prefab)
        {
            if (particlePools[prefab].Count > 0)
                return particlePools[prefab].Dequeue();

            return CreatePooledParticle(prefab);
        }

        private void ReturnOldestEffectToPool()
        {
            if (activeEffects.Count > 0)
            {
                var oldestEffect = activeEffects[0];
                activeEffects.RemoveAt(0);
                ReturnEffectToPool(oldestEffect);
            }
        }

        private void ReturnEffectToPool(GameObject effect)
        {
            effect.SetActive(false);
            if (useObjectPool)
            {
                foreach (var pool in effectPools)
                {
                    if (effect.name.Contains(pool.Key.name))
                    {
                        pool.Value.Enqueue(effect);
                        return;
                    }
                }
            }
            else
            {
                Destroy(effect);
            }
        }

        private void ReturnParticleToPool(ParticleSystem particle)
        {
            particle.gameObject.SetActive(false);
            if (useObjectPool)
            {
                foreach (var pool in particlePools)
                {
                    if (particle.name.Contains(pool.Key.name))
                    {
                        pool.Value.Enqueue(particle);
                        return;
                    }
                }
            }
            else
            {
                Destroy(particle.gameObject);
            }
        }

        private System.Collections.IEnumerator DeactivateAfterDelay(GameObject effect, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (effect != null)
            {
                activeEffects.Remove(effect);
                ReturnEffectToPool(effect);
            }
        }

        private System.Collections.IEnumerator DeactivateParticleAfterDelay(ParticleSystem particle, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (particle != null)
            {
                activeParticles.Remove(particle);
                ReturnParticleToPool(particle);
            }
        }

        private void OnDisable()
        {
            foreach (var effect in activeEffects.ToArray())
            {
                ReturnEffectToPool(effect);
            }
            activeEffects.Clear();

            foreach (var particle in activeParticles.ToArray())
            {
                ReturnParticleToPool(particle);
            }
            activeParticles.Clear();
        }
    }
}