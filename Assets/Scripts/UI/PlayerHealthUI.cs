using UnityEngine;
using ImPulse.Core;
using ImPulse.Player;

namespace ImPulse.UI
{
    public class PlayerHealthUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MaskedBar healthBar;
        
        PlayerStatus player;

        private void OnEnable()
        {
            player = FindAnyObjectByType<PlayerStatus>();

            if (player != null)
            {
                player.OnHealthChanged += HandleHealthChanged;
                
                if (healthBar != null && player.maxHealth > 0f)
                {
                    float initialT = player.currentHealth / player.maxHealth;
                    healthBar.SetFillInstant(initialT);
                }
            }
        }

        private void OnDisable()
        {
            if (player != null)
            {
                player.OnHealthChanged -= HandleHealthChanged;
            }
        }

        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            if (healthBar == null || maxHealth <= 0f) return;

            float healthRatio = currentHealth / maxHealth;
            
            healthBar.SetFill(healthRatio);
        }
    }
}