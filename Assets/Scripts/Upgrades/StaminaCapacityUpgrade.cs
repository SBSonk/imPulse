using UnityEngine;
using ImPulse.Core;
using ImPulse.Player;

namespace ImPulse.Upgrades
{
    [CreateAssetMenu(fileName = "New Stamina Capacity Upgrade", menuName = "ImPulse/Upgrades/Movement/Stamina Capacity")]
    public class StaminaCapacityUpgrade : UpgradeData
    {
        [Header("Capacity Tiers")]
        [Tooltip("Total extra stamina at each level. Example: Lvl 1 (+0.25), Lvl 2 (+0.5), Lvl 3 (+1.0)")]
        public float[] extraStaminaByLevel = { 0.25f, 0.5f, 1.0f };

        public override void ApplyUpgrade(GameObject player, int currentLevel)
        {
            if (player.TryGetComponent<PlayerMovement>(out var movement))
            {
                // Clamp index to prevent out-of-bounds if upgraded beyond expected max level
                int index = Mathf.Clamp(currentLevel - 1, 0, extraStaminaByLevel.Length - 1);
                float bonusStamina = extraStaminaByLevel[index];
                
                movement.SetBonusMaxStamina(bonusStamina);
            }
        }
    }
}