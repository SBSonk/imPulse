using UnityEngine;
using ImPulse.Core;
using System.Collections.Generic;
using ImPulse.Upgrades;

namespace ImPulse.UI
{
    public class UpgradeUIManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private MenuPanelLerper menuLerper;
        [SerializeField] private MenuPanelLerper uiLerper;
        [SerializeField] private UIFader backgroundFader;

        [Header("Upgrade Spawning")]
        [SerializeField] private GameObject upgradeButtonPrefab;
        [SerializeField] private Transform upgradeButtonContainer;

        private List<UpgradeButton> _activeButtons = new List<UpgradeButton>();

        private void Start()
        {
            if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();

            if (gameManager != null)
            {
                gameManager.OnLevelUp += HandleLevelUp;
            }
        }

        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnLevelUp -= HandleLevelUp;
            }
            ClearButtons();
        }

        private void HandleLevelUp(int newLevel)
        {
            if (UpgradeManager.Instance == null) return;

            List<UpgradeData> upgrades = UpgradeManager.Instance.GetUpgradesForLevel();

            if (upgrades.Count > 0)
            {
                SpawnButtons(upgrades);
                backgroundFader.FadeTo(1f);
                menuLerper.SetState(true);
                uiLerper.SetState(false);
            }
            else
            {
                if (gameManager != null) gameManager.FinishedSelecting();
            }
        }

        private void SpawnButtons(List<UpgradeData> upgrades)
        {
            ClearButtons();

            foreach (UpgradeData upgrade in upgrades)
            {
                GameObject btnObj = Instantiate(upgradeButtonPrefab, upgradeButtonContainer);
                UpgradeButton upgradeButton = btnObj.GetComponent<UpgradeButton>();

                if (upgradeButton != null)
                {
                    string displayName = upgrade.upgradeName;

                    // Append Roman numeral if repeatable with a max level (e.g. Endura - I)
                    if (upgrade.isRepeatable && !upgrade.isInfinitelyRepeatable && upgrade.maxLevel > 1)
                    {
                        UpgradeState state = UpgradeManager.Instance.GetUpgradeState(upgrade);
                        int nextLevel = state.currentLevel + 1;
                        displayName += $" - {ToRoman(nextLevel)}";
                    }
                    else if (upgrade.isInfinitelyRepeatable)
                    {
                        // Optional: just append standard numbers for infinitely scaling upgrades
                        UpgradeState state = UpgradeManager.Instance.GetUpgradeState(upgrade);
                        if (state.currentLevel > 0) displayName += $" +{state.currentLevel}";
                    }

                    upgradeButton.SetData(displayName, upgrade.description);
                    
                    UpgradeData capturedUpgrade = upgrade;
                    upgradeButton.OnClick += () => HandleUpgradeSelected(capturedUpgrade);
                    
                    _activeButtons.Add(upgradeButton);
                }
            }
        }

        private void HandleUpgradeSelected(UpgradeData selectedUpgrade)
        {
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.ApplyUpgrade(selectedUpgrade);
            }
            
            backgroundFader.FadeTo(0f);
            menuLerper.SetState(false);
            uiLerper.SetState(true);
            
            if (gameManager != null)
            {
                gameManager.FinishedSelecting();
            }
        }

        private void ClearButtons()
        {
            foreach (var btn in _activeButtons)
            {
                if (btn != null)
                {
                    btn.OnClick = null; 
                    Destroy(btn.gameObject);
                }
            }
            _activeButtons.Clear();
        }

        private string ToRoman(int number)
        {
            if (number < 1) return string.Empty;
            if (number >= 4000) return number.ToString(); 

            string[] romanNumerals = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
            int[] values = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
            string result = "";

            for (int i = 0; i < 13; i++)
            {
                while (number >= values[i])
                {
                    number -= values[i];
                    result += romanNumerals[i];
                }
            }

            return result;
        }
    }
}