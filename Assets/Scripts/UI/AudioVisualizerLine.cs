using UnityEngine;

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

        [SerializeField, Range(0, 1)] float _t = 1;

        public void SetHealth(float t) => _t = t;

        void Start()
        {
            _spectrum = new float[sampleCount];
            _smoothedY = new float[points];
            
            lineRenderer.positionCount = points;
        }

        void Update()
        {
            musicSource.GetSpectrumData(_spectrum, 0, fftWindow);

            int pointSampleWidth = sampleCount / points; 
            float pointSpacing = lineWidth / (points - 1f); 

            for (int i = 0; i < points; i++) 
            {
                float averageSpectrum = 0;
                int samplesUsed = 0;
                
                int startSample = i * pointSampleWidth;
                int endSample = Mathf.Min((i + 1) * pointSampleWidth, sampleCount);

                for (int sample = startSample; sample < endSample; sample++)
                {
                    averageSpectrum += _spectrum[sample];
                    samplesUsed++;
                }
                
                if (samplesUsed > 0)
                {
                    averageSpectrum /= samplesUsed;
                }
                
                float normalizedPosition = i / (points - 1f);
                float bellMultiplier = Mathf.Sin(normalizedPosition * Mathf.PI);

                float targetY = averageSpectrum * heightMultiplier * bellMultiplier * _t;

                targetY = Mathf.Clamp(targetY, 0, maxAmplitude);

                _smoothedY[i] = Mathf.Lerp(_smoothedY[i], targetY, Time.deltaTime * smoothSpeed);

                float xPos = (i * pointSpacing) - (lineWidth / 2f); 
                
                lineRenderer.SetPosition(i, new Vector3(xPos, _smoothedY[i], 0));
            }
        }
    }
}