using UnityEngine;
using ImPulse.Core;

namespace ImPulse.UI
{
    public class AudioVisualizerUI : MonoBehaviour
    {
        [Header("Sample Count must be a power of 2.")]
        [SerializeField] int sampleCount = 256;
        [SerializeField] FFTWindow fftWindow = FFTWindow.BlackmanHarris;
        
        [Header("Wave Settings")]
        [SerializeField] float lineWidth = 1f;
        [SerializeField] int points = 36;
        [SerializeField] float maxAmplitude = 1f;
        
        [Header("Visual Tuning")]
        [SerializeField] float heightMultiplier = 50f;
        [SerializeField, Range(1f, 50f)] float smoothSpeed = 15f;

        [SerializeField] LineRenderer lineRenderer;
        [SerializeField] AudioSource musicSource;

        float[] _spectrum;
        float[] _smoothedY;
        float _basePitch = 0.8f;

        [SerializeField, Range(0, 1)] float _t = 1;

        public void SetHealth(float t) => _t = t;

        void Start()
        {
            _spectrum = new float[sampleCount];
            _smoothedY = new float[points];
            
            lineRenderer.positionCount = points;

            if (GameManager.Instance != null && GameManager.Instance.normalTimescale > 0f)
            {
                _basePitch = GameManager.Instance.normalTimescale;
            }
            else if (musicSource != null && musicSource.pitch > 0f)
            {
                _basePitch = musicSource.pitch;
            }
        }

        void Update()
        {
            if (musicSource == null) return;

            musicSource.GetSpectrumData(_spectrum, 0, fftWindow);

            float currentVolume = musicSource.volume;
            float currentPitch = Mathf.Max(musicSource.pitch, 0.01f);

            // Ratio of current audio pitch compared to the starting normal timescale
            float pitchRatio = Mathf.Clamp(currentPitch / _basePitch, 0.05f, 3f);

            // Scale the active sample range by pitchRatio so points always read the same frequencies
            float effectiveSampleCount = Mathf.Clamp(sampleCount * pitchRatio, 1f, sampleCount);
            float pointSampleWidth = effectiveSampleCount / points; 
            float pointSpacing = lineWidth / (points - 1f); 

            // When frequencies compress into fewer bins at low pitch, per-bin energy concentrates by 1/sqrt(pitchRatio).
            // Multiplying by sqrt(pitchRatio) keeps the peak amplitude consistent with normal speed.
            float pitchEnergyCompensation = Mathf.Sqrt(pitchRatio);

            for (int i = 0; i < points; i++) 
            {
                float startBin = i * pointSampleWidth;
                float endBin = (i + 1) * pointSampleWidth;

                float averageSpectrum = GetInterpolatedSpectrumAverage(startBin, endBin);

                // 1. Normalize against AudioSource volume (handles both level-up ducking and master volume slider)
                if (currentVolume > 0.0001f)
                {
                    averageSpectrum /= currentVolume;
                }

                // 2. Normalize against FFT bin compression from pitch shifting
                averageSpectrum *= pitchEnergyCompensation;
                
                float normalizedPosition = i / (points - 1f);
                float bellMultiplier = Mathf.Sin(normalizedPosition * Mathf.PI);

                float targetY = averageSpectrum * heightMultiplier * bellMultiplier * _t;
                targetY = Mathf.Clamp(targetY, 0f, maxAmplitude);

                _smoothedY[i] = Mathf.Lerp(_smoothedY[i], targetY, Time.unscaledDeltaTime * smoothSpeed);

                float xPos = (i * pointSpacing) - (lineWidth / 2f); 
                lineRenderer.SetPosition(i, new Vector3(xPos, _smoothedY[i], 0f));
            }
        }

        /// <summary>
        /// Reads across fractional FFT bin ranges so low pitch ratios don't suffer from integer rounding.
        /// </summary>
        private float GetInterpolatedSpectrumAverage(float startBin, float endBin)
        {
            int minBin = Mathf.Clamp(Mathf.FloorToInt(startBin), 0, sampleCount - 1);
            int maxBin = Mathf.Clamp(Mathf.CeilToInt(endBin) - 1, minBin, sampleCount - 1);

            if (minBin == maxBin)
            {
                return _spectrum[minBin];
            }

            float sum = 0f;
            float totalWeight = 0f;

            for (int b = minBin; b <= maxBin; b++)
            {
                float overlap = Mathf.Min(endBin, b + 1f) - Mathf.Max(startBin, b);
                if (overlap > 0f)
                {
                    sum += _spectrum[b] * overlap;
                    totalWeight += overlap;
                }
            }

            return totalWeight > 0f ? (sum / totalWeight) : _spectrum[minBin];
        }
    }
}