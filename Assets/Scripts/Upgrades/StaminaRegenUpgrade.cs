using UnityEngine;
using ImPulse.Core;
using ImPulse.Player;

namespace ImPulse.Upgrades
{
    [CreateAssetMenu(fileName = "New Stamina Regen Upgrade", menuName = "ImPulse/Upgrades/Movement/Stamina Regen")]
    public class StaminaRegenUpgrade : UpgradeData
    {
        [Header("Regen Rate Tiers")]
        [Tooltip("Absolute stamina regen rate at each level. Example: Lvl 1 (0.6), Lvl 2 (0.8), Lvl 3 (1.0)")]
        public float[] regenRatesByLevel = { 0.6f, 0.8f, 1.0f };

        public override void ApplyUpgrade(GameObject player, int currentLevel)
        {
            if (player.TryGetComponent<PlayerMovement>(out var movement))
            {
                int index = Mathf.Clamp(currentLevel - 1, 0, regenRatesByLevel.Length - 1);
                float newRegenRate = regenRatesByLevel[index];
                
                movement.SetStaminaRegenRate(newRegenRate);
            }
        }
    }
}