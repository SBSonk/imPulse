using UnityEngine;
using ImPulse.Core;
using ImPulse.Player;
using ImPulse.Weapons;

namespace ImPulse.Upgrades
{
    [CreateAssetMenu(fileName = "New Sleight Upgrade", menuName = "ImPulse/Upgrades/Combat/Sleight Upgrade")]
    public class SleightUpgrade : UpgradeData
    {
        [Header("Sleight Settings")]
        public SleightType sleightType;
        
        public Weapon[] weaponTiers;

        public override void ApplyUpgrade(GameObject player, int currentLevel)
        {
            if (player.TryGetComponent<PlayerAttack>(out var playerAttack))
            {
                playerAttack.UnlockSleight(sleightType);

                if (weaponTiers != null && weaponTiers.Length > 0)
                {
                    int index = GetTierIndex(currentLevel, weaponTiers.Length);
                    Weapon upgradedWeapon = weaponTiers[index];
                    
                    playerAttack.UpdateSleightWeapon(sleightType, upgradedWeapon);
                    UnityEngine.Debug.Log($"[SleightUpgrade] {sleightType} is now Level {currentLevel}, using weapon: {upgradedWeapon.weaponName}");
                }
            }
        }
    }
}