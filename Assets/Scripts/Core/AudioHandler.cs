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
        private double _dspSongStartTime;
        private double _secPerBeat;
        
        private bool _hasSongStarted = false;
        private int _nextBeatIndex = 0;
        private int _nextHalfBeatIndex = 0;
        private int _nextQuarterBeatIndex = 0;

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.clip = trackClip;
            _audioSource.pitch = playbackSpeed; 

            bpm = bpm * playbackSpeed;

            _secPerBeat = 60.0 / bpm;
            _dspSongStartTime = AudioSettings.dspTime + startOffset;

            _audioSource.PlayScheduled(_dspSongStartTime);
        }

        void Update()
        {
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