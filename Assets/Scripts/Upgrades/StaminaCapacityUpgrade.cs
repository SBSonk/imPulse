using UnityEngine;
using ImPulse.Core;
using ImPulse.Player;

namespace ImPulse.Upgrades
{
    [CreateAssetMenu(fileName = "New Stamina Capacity Upgrade", menuName = "ImPulse/Upgrades/Movement/Stamina Capacity")]
    public class StaminaCapacityUpgrade : UpgradeData
    {
        [Header("Capacity Tiers")]
        public float[] extraStaminaByLevel = { 0.25f, 0.5f, 1.0f };

        public override void ApplyUpgrade(GameObject player, int currentLevel)
        {
            if (player.TryGetComponent<PlayerMovement>(out var movement))
            {
                int index = GetTierIndex(currentLevel, extraStaminaByLevel.Length);
                float bonusStamina = extraStaminaByLevel[index];
                
                movement.SetBonusMaxStamina(bonusStamina);
            }
        }
    }
}