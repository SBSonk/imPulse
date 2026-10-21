using UnityEngine;
using ImPulse.Core;
using ImPulse.Player;

namespace ImPulse.Upgrades
{
    [CreateAssetMenu(fileName = "New Health Capacity Upgrade", menuName = "ImPulse/Upgrades/Status/Health Capacity")]
    public class HealthCapacityUpgrade : UpgradeData
    {
        [Header("Health Tiers")]
        public float[] maxHealthByLevel = { 60f, 75f, 100f };

        public override void ApplyUpgrade(GameObject player, int currentLevel)
        {
            if (player.TryGetComponent<PlayerStatus>(out var status))
            {
                int index = Mathf.Clamp(currentLevel - 1, 0, maxHealthByLevel.Length - 1);
                float newMaxHealth = maxHealthByLevel[index];
                
                status.SetMaxHealth(newMaxHealth);
            }
        }
    }
}