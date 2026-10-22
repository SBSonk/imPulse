using UnityEngine;
using ImPulse.Player;
using ImPulse.Weapons;

namespace ImPulse.Upgrades
{
    [CreateAssetMenu(fileName = "New Weapon Upgrade", menuName = "ImPulse/Upgrades/Combat/Weapon Upgrade")]
    public class WeaponUpgrade : UpgradeData
    {
        [Header("Weapon Settings")]
        public WeaponSlot slotToUpgrade;
        
        public Weapon[] weaponTiers;

        public override void ApplyUpgrade(GameObject player, int currentLevel)
        {
            if (player.TryGetComponent<PlayerAttack>(out var playerAttack))
            {
                if (weaponTiers != null && weaponTiers.Length > 0)
                {
                    int index = GetTierIndex(currentLevel, weaponTiers.Length);
                    Weapon upgradedWeapon = weaponTiers[index];
                    
                    playerAttack.ChangeWeapon(slotToUpgrade, upgradedWeapon);
                    
                    UnityEngine.Debug.Log($"[WeaponUpgrade] {slotToUpgrade} weapon is now Level {currentLevel}, using: {upgradedWeapon.weaponName}");
                }
            }
        }
    }
}