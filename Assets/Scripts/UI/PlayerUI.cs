using UnityEngine;
using ImPulse.Core;
using ImPulse.Player;
using TMPro;

namespace ImPulse.UI
{
    public class PlayerUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MaskedBar staminaBar;
        [SerializeField] private AudioVisualizerUI audioVisualizer;
        [SerializeField] TextMeshProUGUI scoreText, roundText;
        
        PlayerStatus playerStatus;
        PlayerMovement playerMovement;

        void Start()
        {
            GameManager.Instance.OnScoreChanged += HandleScoreChange;
            EnemyWaveSpawner.Instance.OnRoundChange += HandleRoundChange;

            playerStatus = FindAnyObjectByType<PlayerStatus>();
            playerMovement = FindAnyObjectByType<PlayerMovement>();

            // set instant for init
            if (playerStatus != null)
            {
                playerStatus.OnHealthChanged += HandleHealthChanged;
                
                if (audioVisualizer != null && playerStatus.maxHealth > 0f)
                {
                    float healthT = playerStatus.currentHealth / playerStatus.maxHealth;
                    audioVisualizer.SetHealth(healthT);
                }
            }

            if (playerMovement != null && staminaBar != null)
            {
                float maxStam = playerMovement.GetMaxStamina();
                if (maxStam > 0f)
                {
                    staminaBar.SetFillInstant(playerMovement.GetCurrentStamina() / maxStam);
                }
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnScoreChanged -= HandleScoreChange;
            if (EnemyWaveSpawner.Instance != null) EnemyWaveSpawner.Instance.OnRoundChange -= HandleRoundChange;

            if (playerStatus != null)
            {
                playerStatus.OnHealthChanged -= HandleHealthChanged;
            }
        }

        private void Update()
        {
            if (playerMovement != null && staminaBar != null)
            {
                float maxStamina = playerMovement.GetMaxStamina();
                if (maxStamina > 0f)
                {
                    float staminaRatio = playerMovement.GetCurrentStamina() / maxStamina;
                    staminaBar.SetFill(staminaRatio);
                }
            }
        }

        void HandleScoreChange(int newScore)
        {
            scoreText.SetText(newScore.ToString("00000"));
        }

        void HandleRoundChange(int newRound)
        {
            roundText.SetText($"round {newRound}.");
        }

        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            if (audioVisualizer == null || maxHealth <= 0f) return;

            float healthRatio = currentHealth / maxHealth;
            audioVisualizer.SetHealth(healthRatio);
        }
    }
}