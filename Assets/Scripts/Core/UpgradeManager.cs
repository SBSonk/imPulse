using UnityEngine;
using System.Collections.Generic;
using ImPulse.Upgrades;
using ImPulse.Player;
using System;

namespace ImPulse.Core
{
    public struct UpgradeState
    {
        public int currentLevel;
        public bool isRetired;
    }

    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        [Header("Upgrade Configuration")]
        public int maxUpgradeChoices = 3;
        
        public List<UpgradeData> availableUpgrades = new List<UpgradeData>(); 

        private Dictionary<UpgradeData, UpgradeState> _upgradeStates = new Dictionary<UpgradeData, UpgradeState>();
        
        public Action OnFinishedSelecting;

        GameObject _playerInstance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeStates();
            }
            else
            {
                Destroy(gameObject);
            }

            _playerInstance = FindAnyObjectByType<PlayerStatus>().gameObject;
        }

        private void InitializeStates()
        {
            foreach (var upgrade in availableUpgrades)
            {
                if (upgrade != null && !_upgradeStates.ContainsKey(upgrade))
                {
                    _upgradeStates.Add(upgrade, new UpgradeState { currentLevel = 0, isRetired = false });
                }
            }
        }

        public List<UpgradeData> GetUpgradesForLevel()
        {
            List<UpgradeData> validUpgrades = new List<UpgradeData>();

            foreach (var upgrade in availableUpgrades)
            {
                if (upgrade != null && _upgradeStates.TryGetValue(upgrade, out UpgradeState state))
                {
                    if (!state.isRetired)
                    {
                        validUpgrades.Add(upgrade);
                    }
                }
            }

            List<UpgradeData> choices = new List<UpgradeData>();
            if (validUpgrades.Count == 0) return choices;

            int amount = Mathf.Min(maxUpgradeChoices, validUpgrades.Count);
            
            for (int i = 0; i < amount; i++)
            {
                choices.Add(validUpgrades[i]);
            }

            return choices;
        }

        public void ApplyUpgrade(UpgradeData upgrade)
        {
            if (upgrade == null) return;

            if (_playerInstance != null)
            {
                if (_upgradeStates.TryGetValue(upgrade, out UpgradeState state))
                {
                    state.currentLevel++;

                    // Mark as retired only if it isn't infinitely repeatable and has reached its cap
                    if (!upgrade.isRepeatable || (!upgrade.isInfinitelyRepeatable && state.currentLevel >= upgrade.maxLevel))
                    {
                        state.isRetired = true;
                    }

                    _upgradeStates[upgrade] = state;
                    upgrade.ApplyUpgrade(_playerInstance, state.currentLevel);
                    OnFinishedSelecting?.Invoke();
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[UpgradeManager] Cannot apply {upgrade.upgradeName}: Player instance is not assigned.");
            }
        }

        public UpgradeState GetUpgradeState(UpgradeData upgrade)
        {
            if (_upgradeStates.TryGetValue(upgrade, out UpgradeState state))
            {
                return state;
            }
            return new UpgradeState { currentLevel = 0, isRetired = false };
        }
    }
}