using UnityEngine;
using System;

namespace ImPulse.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        private AudioHandler _audioHandler;
        
        [Range(0.1f, 2f)] public float normalTimescale = .8f, levelUpTimescale = .2f;
        [Range(0f, 1f)] public float normalVolume = .8f, levelUpVolume = .2f;
        
        float _timeScale = .8f;

        [Header("Experience & Leveling")]
        public int currentLevel = 1;
        public int currentXP = 0;
        public int xpToNextLevel = 100;
        public float xpScaleFactor = 1.5f;

        [Header("Scoring")]
        public int score = 0;

        // Events for UI
        public event Action<int, int> OnXPChanged;
        public event Action<int> OnScoreChanged;
        public event Action<int> OnLevelUp;

        void Awake()
        {
            if (!Instance) Instance = this;
            else Destroy(gameObject);
            _audioHandler = FindAnyObjectByType<AudioHandler>();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            SetTimescale(normalTimescale);
        }

        public void AddXP(int amount)
        {
            currentXP += amount;
            
            OnXPChanged?.Invoke(currentXP, xpToNextLevel);

            if (currentXP >= xpToNextLevel)
            {
                TriggerLevelUp();
            }
        }

        public void AddScore(int amount)
        {
            score += amount;

            OnScoreChanged?.Invoke(score);
        }

        private void TriggerLevelUp()
        {
            currentLevel++;
            currentXP -= xpToNextLevel; // Carry over excess XP
            xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpScaleFactor);
            
            OnXPChanged?.Invoke(currentXP, xpToNextLevel);
            OnLevelUp?.Invoke(currentLevel);

            // Put game in slow motion
            SetTimescale(levelUpTimescale);
            _audioHandler.SetVolume(levelUpVolume);
        }

        public void FinishedSelecting()
        {
            SetTimescale(normalTimescale);
            _audioHandler.SetVolume(normalVolume);
        }

        public void SetTimescale(float timeScale)
        {
            _timeScale = timeScale;

            Time.timeScale = timeScale;
            _audioHandler.SetPlaybackSpeed(timeScale);
        }
    }
}