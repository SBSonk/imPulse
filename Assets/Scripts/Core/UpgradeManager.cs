using UnityEngine;
using ImPulse.Core;
using ImPulse.UI;

namespace ImPulse.Core
{
    public class UpgradeManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private MenuPanelLerper menuLerper, uiLerper;
        [SerializeField] private UIFader backgroundFader;
        [SerializeField] private UpgradeButton[] upgradeButtons;

        private void Start()
        {
            if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();

            if (gameManager != null)
            {
                gameManager.OnLevelUp += HandleLevelUp;
            }

            // Subscribe to all upgrade buttons
            foreach (var btn in upgradeButtons)
            {
                if (btn != null) btn.OnClick += HandleUpgradeSelected;
            }
        }

        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnLevelUp -= HandleLevelUp;
            }

            foreach (var btn in upgradeButtons)
            {
                if (btn != null) btn.OnClick -= HandleUpgradeSelected;
            }
        }

        private void HandleLevelUp(int newLevel)
        {
            // TODO: Populate the upgrade buttons with random stats/weapons here before showing them
            
            backgroundFader.FadeTo(1f);
            menuLerper.SetState(true);
            uiLerper.SetState(false);
        }

        private void HandleUpgradeSelected()
        {
            // TODO: Apply the specific upgrade chosen by the player here
            
            backgroundFader.FadeTo(0f);
            menuLerper.SetState(false);
            uiLerper.SetState(true);
            
            if (gameManager != null)
            {
                gameManager.FinishedSelecting();
            }
        }
    }
}