using UnityEngine;

namespace ImPulse.Upgrades
{
    public abstract class UpgradeData : ScriptableObject
    {
        [Header("UI Information")]
        public string upgradeName = "New Upgrade";
        [TextArea] public string description = "Upgrade Description";
        
        [Header("Progression Settings")]
        public bool isRepeatable = false;
        public bool isInfinitelyRepeatable = false;
        [Tooltip("The maximum level this upgrade can reach. Ignored if isInfinitelyRepeatable is true.")]
        public int maxLevel = 1;

        /// <summary>
        /// Applies the upgrade effects to the player. 
        /// </summary>
        public abstract void ApplyUpgrade(GameObject player, int currentLevel);
    }
}