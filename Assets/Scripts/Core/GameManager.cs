using UnityEngine;

namespace ImPulse.Core
{
    public class GameManager : MonoBehaviour
    {
        AudioHandler _audioHandler;
        [Range(0.1f, 2)]public float timeScale = 1;

        void Awake()
        {
            _audioHandler = FindAnyObjectByType<AudioHandler>();
        }

        void Update()
        {
            Time.timeScale = timeScale;
            _audioHandler.SetPlaybackSpeed(timeScale);
        }
    }
}