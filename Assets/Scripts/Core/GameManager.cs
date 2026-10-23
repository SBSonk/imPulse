using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System;

namespace ImPulse.Core
{
    public enum GameState
    {
        Menu,
        Playing,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        private AudioHandler _audioHandler;
        
        [Header("Game State")]
        public GameState currentState = GameState.Menu;

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
        public event Action<GameState> OnGameStateChanged;

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
            SetGameState(GameState.Menu);
        }

        void Update()
        {
            // Listen for global input to transition out of Menu or Game Over states
            if (currentState == GameState.Menu || currentState == GameState.GameOver)
            {
                bool startInputDetected = false;

                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    startInputDetected = true;
                }
                else if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    startInputDetected = true;
                }

                if (startInputDetected)
                {
                    if (currentState == GameState.Menu) StartGame();
                    else if (currentState == GameState.GameOver) RestartGame();
                }
            }
        }

        public void SetGameState(GameState newState)
        {
            currentState = newState;
            OnGameStateChanged?.Invoke(currentState);

            switch (currentState)
            {
                case GameState.Menu:
                    SetTimescale(0);
                    if (_audioHandler != null) _audioHandler.SetVolume(0f);
                    break;
                case GameState.Playing:
                    SetTimescale(normalTimescale);
                    if (_audioHandler != null) _audioHandler.SetVolume(normalVolume);
                    break;
                case GameState.GameOver:
                    SetTimescale(0);
                    if (_audioHandler != null) _audioHandler.SetVolume(0f);
                    break;
            }
        }

        public void StartGame()
        {
            if (_audioHandler != null) _audioHandler.StartPlaying();
            SetGameState(GameState.Playing);
        }

        public void TriggerGameOver()
        {
            if (currentState == GameState.GameOver) return;
            SetGameState(GameState.GameOver);
        }

        public void RestartGame()
        {
            Time.timeScale = 1f; 
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
            currentXP -= xpToNextLevel; 
            xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpScaleFactor);
            
            OnXPChanged?.Invoke(currentXP, xpToNextLevel);
            OnLevelUp?.Invoke(currentLevel);

            SetTimescale(levelUpTimescale);
            if (_audioHandler != null) _audioHandler.SetVolume(levelUpVolume);
        }

        public void FinishedSelecting()
        {
            SetTimescale(normalTimescale);
            if (_audioHandler != null) _audioHandler.SetVolume(normalVolume);
        }

        public void SetTimescale(float timeScale)
        {
            _timeScale = timeScale;
            Time.timeScale = timeScale;
            if (_audioHandler != null) _audioHandler.SetPlaybackSpeed(timeScale);
        }
    }
}