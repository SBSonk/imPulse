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
        public int statUpgradeChoices = 2;
        public int weaponUpgradeChoices = 2;
        
        [Header("Upgrade Pools")]
        public List<UpgradeData> statUpgrades = new List<UpgradeData>(); 
        public List<UpgradeData> weaponUpgrades = new List<UpgradeData>(); 

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
            InitializePool(statUpgrades);
            InitializePool(weaponUpgrades);
        }

        private void InitializePool(List<UpgradeData> pool)
        {
            foreach (var upgrade in pool)
            {
                if (upgrade != null && !_upgradeStates.ContainsKey(upgrade))
                {
                    _upgradeStates.Add(upgrade, new UpgradeState { currentLevel = 0, isRetired = false });
                }
            }
        }

        public List<UpgradeData> GetUpgradesForLevel()
        {
            List<UpgradeData> validStats = GetValidUpgrades(statUpgrades);
            List<UpgradeData> validWeapons = GetValidUpgrades(weaponUpgrades);

            ShuffleList(validStats);
            ShuffleList(validWeapons);

            List<UpgradeData> choices = new List<UpgradeData>();

            int statsToPull = Mathf.Min(statUpgradeChoices, validStats.Count);
            for (int i = 0; i < statsToPull; i++)
            {
                choices.Add(validStats[i]);
            }

            int weaponsToPull = Mathf.Min(weaponUpgradeChoices, validWeapons.Count);
            for (int i = 0; i < weaponsToPull; i++)
            {
                choices.Add(validWeapons[i]);
            }

            return choices;
        }

        private List<UpgradeData> GetValidUpgrades(List<UpgradeData> pool)
        {
            List<UpgradeData> validUpgrades = new List<UpgradeData>();
            foreach (var upgrade in pool)
            {
                if (upgrade != null && _upgradeStates.TryGetValue(upgrade, out UpgradeState state))
                {
                    if (!state.isRetired)
                    {
                        validUpgrades.Add(upgrade);
                    }
                }
            }
            return validUpgrades;
        }

        private void ShuffleList(List<UpgradeData> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                UpgradeData temp = list[i];
                int randomIndex = UnityEngine.Random.Range(i, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        public void ApplyUpgrade(UpgradeData upgrade)
        {
            if (upgrade == null) return;

            if (_playerInstance != null)
            {
                if (_upgradeStates.TryGetValue(upgrade, out UpgradeState state))
                {
                    state.currentLevel++;

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