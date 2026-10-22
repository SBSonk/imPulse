using UnityEngine;
using ImPulse.Core;
using TMPro;

namespace ImPulse.UI
{
    public class XPIndicatorUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MaskedBar xpBar;
        [SerializeField] private TextMeshProUGUI levelText;

        private GameManager _gameManager;
        private int _lastLevel = 1;

        private void Start()
        {
            _gameManager = FindAnyObjectByType<GameManager>();

            if (_gameManager != null)
            {
                _gameManager.OnXPChanged += HandleXPChanged;
                _gameManager.OnLevelUp += HandleLevelUp;

                UpdateUI(_gameManager.currentXP, _gameManager.xpToNextLevel, _gameManager.currentLevel);
            }
        }

        private void OnDestroy()
        {
            if (_gameManager != null)
            {
                _gameManager.OnXPChanged -= HandleXPChanged;
                _gameManager.OnLevelUp -= HandleLevelUp;
            }
        }

        private void HandleXPChanged(int currentXP, int xpToNext)
        {
            float fillAmount = xpToNext > 0 ? (float)currentXP / xpToNext : 1f;
            
            if (xpBar != null)
            {
                xpBar.SetFill(fillAmount);
            }
        }

        private void HandleLevelUp(int newLevel)
        {
            if (levelText != null)
            {
                levelText.text = $"Lv. {newLevel}";
            }

            if (xpBar != null)
            {
                xpBar.SetFillInstant(0f);
            }
            
            _lastLevel = newLevel;
        }

        private void UpdateUI(int currentXP, int xpToNext, int level)
        {
            HandleLevelUp(level);
            HandleXPChanged(currentXP, xpToNext);
        }
    }
}