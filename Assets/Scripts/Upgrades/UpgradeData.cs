using UnityEngine;

namespace ImPulse.Upgrades
{
    public abstract class UpgradeData : ScriptableObject
    {
        [Header("UI Information")]
        public string upgradeName = "New Upgrade";
        [TextArea] public string[] descriptions = { "Upgrade Description" };
        
        [Header("Progression Settings")]
        public bool isRepeatable = false;
        public bool isInfinitelyRepeatable = false;
        [Tooltip("The maximum level this upgrade can reach. Ignored if isInfinitelyRepeatable is true.")]
        public int maxLevel = 1;

        public abstract void ApplyUpgrade(GameObject player, int currentLevel);

        protected int GetTierIndex(int currentLevel, int arrayLength)
        {
            if (arrayLength <= 0) return 0;
            return Mathf.Clamp(currentLevel - 1, 0, arrayLength - 1);
        }

        public string GetDescription(int nextLevel)
        {
            if (descriptions == null || descriptions.Length == 0) return string.Empty;
            
            int index = GetTierIndex(nextLevel, descriptions.Length);
            return descriptions[index];
        }
    }
}