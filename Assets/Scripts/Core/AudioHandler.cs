using System;
using UnityEngine;
using UnityEngine.Audio;

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
        
        [Tooltip("Global multiplier for the music volume controlled by the player.")]
        [Range(0f, 1f)] public float masterVolume = 1f;

        [Header("Rhythm Pattern")]
        public PatternResolution patternResolution = PatternResolution.Whole;
        public bool[] currentPattern = { true, false, true, false };

        [SerializeField] AudioMixer mixer;
                
        [Header("Playback State (Read Only)")]
        public double songPositionInSeconds;
        public double songPositionInBeats;

        public event Action OnSongStart;
        
        public event Action<int> OnBeat;
        public event Action<int> OnHalfBeat;
        public event Action<int> OnQuarterBeat;

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
            float actualTargetVolume = _targetVolume/* * masterVolume*/;

            if (!Mathf.Approximately(_audioSource.volume, actualTargetVolume))
            {
                _audioSource.volume = Mathf.MoveTowards(_audioSource.volume, actualTargetVolume, Time.unscaledDeltaTime * volumeFadeSpeed);
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

        public void SetPlaybackSpeed(float newSpeed)
        {
            playbackSpeed = Mathf.Clamp(newSpeed, 0.1f, 3f);
            
            if (_audioSource != null)
            {
                _audioSource.pitch = playbackSpeed;
            }

            bpm = _baseBpm * playbackSpeed;
            _secPerBeat = 60.0 / bpm;

            if (_hasSongStarted && _isStarted)
            {
                _dspSongStartTime = AudioSettings.dspTime - (songPositionInBeats * _secPerBeat);
            }
        }

        // for setting the actual volume of the audio source, use SetMasterVolume for output volume
        public void SetVolume(float newVolume)
        {
            _targetVolume = newVolume;
            _targetVolume = Mathf.Clamp(_targetVolume, 0.00001f, 1f); // low but not 0 so that we have smth to amplify for the visualizer
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume); 
            
            if (mixer != null)
            {
                float dB = masterVolume > 0.0001f ? Mathf.Log10(masterVolume) * 20f : -80f;
                mixer.SetFloat("Master", dB);
            }
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