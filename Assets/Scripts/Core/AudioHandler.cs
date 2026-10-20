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
        
        // Fired on every whole beat
        public event Action<int> OnBeat;
        
        // Fired on every half beat 
        public event Action<int> OnHalfBeat;

        private AudioSource _audioSource;
        private double _dspSongStartTime;
        private double _secPerBeat;
        
        private bool _hasSongStarted = false;
        private int _nextBeatIndex = 0;
        private int _nextHalfBeatIndex = 0;

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.clip = trackClip;
            _audioSource.pitch = playbackSpeed; 

            // Adjust the actual BPM mathematically based on the playback speed.
            bpm = bpm * playbackSpeed;

            _secPerBeat = 60.0 / bpm;
            _dspSongStartTime = AudioSettings.dspTime + startOffset;

            _audioSource.PlayScheduled(_dspSongStartTime);
        }

        void Update()
        {
            // Because our BPM is already scaled, we use pure real-world elapsed time here.
            // (No double-multiplying by playbackSpeed!)
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
                    _nextBeatIndex++;
                }

                while (songPositionInBeats >= _nextHalfBeatIndex * 0.5)
                {
                    OnHalfBeat?.Invoke(_nextHalfBeatIndex);
                    _nextHalfBeatIndex++;
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