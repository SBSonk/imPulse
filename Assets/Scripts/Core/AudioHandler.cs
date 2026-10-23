using System;
using UnityEngine;

namespace ImPulse.Core
{
    public enum PatternResolution
    {
        Whole,
        Half,
        Quarter
    }

    [RequireComponent(typeof(AudioSource))]
    public class AudioHandler : MonoBehaviour
    {
        [Header("Track Configuration")]
        public AudioClip trackClip;
        public double bpm = 120.0;
        public double startOffset = 3.0;
        
        [Tooltip("Multiplies the speed of the audio and the beat calculations.")]
        [Range(0.1f, 3f)]
        public float playbackSpeed = 1f;

        [Header("Audio Settings")]
        [Tooltip("How fast the volume lerps to the target value (units per second).")]
        public float volumeFadeSpeed = 2f;

        [Header("Rhythm Pattern")]
        public PatternResolution patternResolution = PatternResolution.Whole;
        public bool[] currentPattern = { true, false, true, false };

        [Header("Playback State (Read Only)")]
        public double songPositionInSeconds;
        public double songPositionInBeats;

        public event Action OnSongStart;
        
        // Fired on every beat regardless of pattern
        public event Action<int> OnBeat;
        public event Action<int> OnHalfBeat;
        public event Action<int> OnQuarterBeat;

        // Fired only when a beat aligns with a 'true' value in the currentPattern array at the chosen resolution
        public event Action<int> OnWholePatternBeat;
        public event Action<int> OnHalfPatternBeat;
        public event Action<int> OnQuarterPatternBeat;

        private AudioSource _audioSource;
        private double _baseBpm;
        private double _dspSongStartTime;
        private double _secPerBeat;
        
        private bool _isStarted = false;
        private bool _hasSongStarted = false;
        private int _nextBeatIndex = 0;
        private int _nextHalfBeatIndex = 0;
        private int _nextQuarterBeatIndex = 0;

        private float _targetVolume;

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.clip = trackClip;
            _targetVolume = _audioSource.volume;
            
            _baseBpm = bpm; 
        }

        public void StartPlaying()
        {
            if (_isStarted) return;
            _isStarted = true;

            _secPerBeat = 60.0 / bpm;
            _dspSongStartTime = AudioSettings.dspTime + startOffset;
            _audioSource.PlayScheduled(_dspSongStartTime);
        }

        void Update()
        {
            // Handle gradual volume lerping using unscaled time (immune to slow-motion)
            if (!Mathf.Approximately(_audioSource.volume, _targetVolume))
            {
                _audioSource.volume = Mathf.MoveTowards(_audioSource.volume, _targetVolume, Time.unscaledDeltaTime * volumeFadeSpeed);
            }

            if (!_isStarted) return;

            songPositionInSeconds = AudioSettings.dspTime - _dspSongStartTime;
            songPositionInBeats = songPositionInSeconds / _secPerBeat;

            if (!_hasSongStarted && songPositionInSeconds >= 0)
            {
                _hasSongStarted = true;
                OnSongStart?.Invoke();
            }

            if (_hasSongStarted)
            {
                while (songPositionInBeats >= _nextBeatIndex)
                {
                    OnBeat?.Invoke(_nextBeatIndex);
                    EvaluatePattern(_nextBeatIndex, PatternResolution.Whole);
                    _nextBeatIndex++;
                }

                while (songPositionInBeats >= _nextHalfBeatIndex * 0.5)
                {
                    OnHalfBeat?.Invoke(_nextHalfBeatIndex);
                    EvaluatePattern(_nextHalfBeatIndex, PatternResolution.Half);
                    _nextHalfBeatIndex++;
                }

                while (songPositionInBeats >= _nextQuarterBeatIndex * 0.25)
                {
                    OnQuarterBeat?.Invoke(_nextQuarterBeatIndex);
                    EvaluatePattern(_nextQuarterBeatIndex, PatternResolution.Quarter);
                    _nextQuarterBeatIndex++;
                }
            }
        }

        /// <summary>
        /// Dynamically updates the audio pitch and recalculates beat timings without breaking the current progression.
        /// </summary>
        public void SetPlaybackSpeed(float newSpeed)
        {
            playbackSpeed = Mathf.Clamp(newSpeed, 0.1f, 3f);
            
            if (_audioSource != null)
            {
                _audioSource.pitch = playbackSpeed;
            }

            bpm = _baseBpm * playbackSpeed;
            _secPerBeat = 60.0 / bpm;

            // If the song is already playing, we must shift the DSP start time backwards or forwards.
            // Otherwise, updating _secPerBeat will instantly drastically alter songPositionInBeats.
            if (_hasSongStarted && _isStarted)
            {
                _dspSongStartTime = AudioSettings.dspTime - (songPositionInBeats * _secPerBeat);
            }
        }

        /// <summary>
        /// Sets the target volume. The audio source will gradually lerp to this value in Update.
        /// </summary>
        public void SetVolume(float newVolume)
        {
            _targetVolume = Mathf.Clamp01(newVolume);
        }

        private void EvaluatePattern(int beatIndex, PatternResolution resolution)
        {
            if (patternResolution == resolution && currentPattern != null && currentPattern.Length > 0)
            {
                int stepIndex = beatIndex % currentPattern.Length;
                if (currentPattern[stepIndex])
                {
                    switch (resolution)
                    {
                        case PatternResolution.Whole: 
                            OnWholePatternBeat?.Invoke(beatIndex); 
                            break;
                        case PatternResolution.Half: 
                            OnHalfPatternBeat?.Invoke(beatIndex); 
                            break;
                        case PatternResolution.Quarter: 
                            OnQuarterPatternBeat?.Invoke(beatIndex); 
                            break;
                    }
                }
            }
        }

        public float GetSongTime()
        {
            float t = (float) songPositionInSeconds;
            return t > 0f ? t : 0f;
        }

        public float GetSecondsPerBeat() => (float) _secPerBeat;

        public float GetFrequency() => (float) (bpm / 60f);
    }
}