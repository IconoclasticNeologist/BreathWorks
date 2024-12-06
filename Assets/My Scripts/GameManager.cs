using UnityEngine;
using TMPro;

namespace MyScripts
{
    public class GameManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private TextMeshProUGUI streakText;

        [Header("Audio")]
        [SerializeField] private AudioClip streakSound;
        [SerializeField] private AudioSource audioSource;

        [Header("Game Settings")]
        [SerializeField] private int pointsPerHit = 100;
        [SerializeField] private int streakBonus = 50;
        [SerializeField] private int streakThreshold = 3;

        [Header("Effects")]
        [SerializeField] private StreakEffectsManager streakEffectsManager;

        private int currentScore = 0;
        private int highScore = 0;
        private int currentStreak = 0;

        private static GameManager instance;
        public static GameManager Instance { get { return instance; } }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            instance = this;

            // Load high score
            highScore = PlayerPrefs.GetInt("HighScore", 0);

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            UpdateUI();
        }

        public void AddPoints(int multiplier = 1)
        {
            currentStreak++;

            // Calculate points including streak bonus
            int points = pointsPerHit * multiplier;
            if (currentStreak >= streakThreshold)
            {
                points += streakBonus * (currentStreak - streakThreshold + 1);
                if (audioSource != null && streakSound != null)
                {
                    audioSource.PlayOneShot(streakSound);
                }
            }

            // Trigger streak effects
            if (streakEffectsManager != null)
            {
                streakEffectsManager.HandleStreak(currentStreak);
            }

            currentScore += points;

            // Update high score if needed
            if (currentScore > highScore)
            {
                highScore = currentScore;
                PlayerPrefs.SetInt("HighScore", highScore);
            }

            UpdateUI();
        }

        public void ResetStreak()
        {
            currentStreak = 0;
            UpdateUI();
        }

        public void ResetScore()
        {
            currentScore = 0;
            currentStreak = 0;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (scoreText != null)
                scoreText.text = $"Score: {currentScore}";

            if (highScoreText != null)
                highScoreText.text = $"High Score: {highScore}";

            if (streakText != null)
                streakText.text = $"Streak: {currentStreak}";
        }

        private void OnDisable()
        {
            PlayerPrefs.Save();
        }
    }
}