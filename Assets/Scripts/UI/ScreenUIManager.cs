using UnityEngine;
using UnityEngine.UI;
using ImPulse.Core;
using TMPro;
using System;

namespace ImPulse.UI
{
    public class ScreenUIManager : MonoBehaviour
    {
        [Header("Screen Faders")]
        [SerializeField] private UIFader startScreenFader;
        [SerializeField] private UIFader deathScreenFader;
        [SerializeField] private UIFader pauseScreenFader;

        [Header("Pause Controls")]
        [SerializeField] private Slider volumeSlider;

        [Header("Death Screen Stats")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI timeText;

        private GameManager _gameManager;
        private AudioHandler _audioHandler;

        private void Start()
        {
            _gameManager = FindAnyObjectByType<GameManager>();
            _audioHandler = FindAnyObjectByType<AudioHandler>();

            if (_audioHandler != null && volumeSlider != null)
            {
                // Sync the slider to the handler's starting value
                volumeSlider.value = _audioHandler.masterVolume;
                // Add a listener so the audio updates in real time as the player drags the slider
                volumeSlider.onValueChanged.AddListener((val) => _audioHandler.SetMasterVolume(val));
            }

            if (_gameManager != null)
            {
                _gameManager.OnGameStateChanged += HandleGameStateChanged;
                HandleGameStateChanged(_gameManager.currentState);
            }
        }

        private void OnDestroy()
        {
            if (_gameManager != null)
            {
                _gameManager.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Menu:
                    if (startScreenFader != null) startScreenFader.FadeTo(1f); 
                    if (deathScreenFader != null) deathScreenFader.FadeTo(0f);
                    if (pauseScreenFader != null) pauseScreenFader.FadeTo(0f);
                    break;

                case GameState.Playing:
                    if (startScreenFader != null) startScreenFader.FadeTo(0f); 
                    if (deathScreenFader != null) deathScreenFader.FadeTo(0f);
                    if (pauseScreenFader != null) pauseScreenFader.FadeTo(0f);
                    break;

                case GameState.Paused:
                    if (startScreenFader != null) startScreenFader.FadeTo(0f); 
                    if (deathScreenFader != null) deathScreenFader.FadeTo(0f);
                    if (pauseScreenFader != null) pauseScreenFader.FadeTo(1f);
                    break;

                case GameState.GameOver:
                    UpdateDeathStats();
                    if (startScreenFader != null) startScreenFader.FadeTo(0f); 
                    if (deathScreenFader != null) deathScreenFader.FadeTo(1f); 
                    if (pauseScreenFader != null) pauseScreenFader.FadeTo(0f);
                    break;
            }
        }

        private void UpdateDeathStats()
        {
            if (_gameManager != null && scoreText != null)
            {
                scoreText.text = $"{_gameManager.score.ToString("00000")}";
            }

            if (EnemyWaveSpawner.Instance != null && roundText != null)
            {
                roundText.text = $"round {EnemyWaveSpawner.Instance.currentRound}";
            }

            if (_audioHandler != null && timeText != null)
            {
                float survivalSeconds = _audioHandler.GetSongTime();
                TimeSpan timeSpan = TimeSpan.FromSeconds(survivalSeconds);
                timeText.text = $"{timeSpan.ToString(@"mm\:ss")}"; 
            }
        }
    }
}