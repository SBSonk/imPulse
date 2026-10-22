using UnityEngine;
using ImPulse.Core;
using ImPulse.Player;

namespace ImPulse.Upgrades
{
    [CreateAssetMenu(fileName = "New Stamina Regen Upgrade", menuName = "ImPulse/Upgrades/Movement/Stamina Regen")]
    public class StaminaRegenUpgrade : UpgradeData
    {
        [Header("Regen Rate Tiers")]
        public float[] regenRatesByLevel = { 0.6f, 0.8f, 1.0f };

        public override void ApplyUpgrade(GameObject player, int currentLevel)
        {
            if (player.TryGetComponent<PlayerMovement>(out var movement))
            {
                int index = GetTierIndex(currentLevel, regenRatesByLevel.Length);
                float newRegenRate = regenRatesByLevel[index];
                
                movement.SetStaminaRegenRate(newRegenRate);
            }
        }
    }
}