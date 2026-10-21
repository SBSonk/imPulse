using UnityEngine;
using ImPulse.Core;

namespace ImPulse.Upgrades
{
    [CreateAssetMenu(fileName = "New Heal Upgrade", menuName = "ImPulse/Upgrades/Heal Upgrade")]
    public class HealUpgrade : UpgradeData
    {
        [Header("Heal Settings")]
        public float healPercentage = 0.5f;

        public override void ApplyUpgrade(GameObject player, int currentLevel)
        {
            if (player.TryGetComponent<HasHealth>(out var playerHealth))
            {
                float healAmount = playerHealth.maxHealth * healPercentage;
                
                playerHealth.Heal(healAmount);
                
                UnityEngine.Debug.Log($"[HealUpgrade] Restored {healAmount} HP. Current Health is now {playerHealth.currentHealth}. (Level {currentLevel})");
            }
        }
    }
}