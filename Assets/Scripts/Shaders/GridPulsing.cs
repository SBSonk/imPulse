using UnityEngine;
using ImPulse.Core;
using ImPulse.Helpers;

namespace ImPulse.Rendering
{
    public class GridPulsing : MonoBehaviour
    {
        [SerializeField] AudioHandler audioHandler;
        [SerializeField] float pulseRadiusInBeats = 0.25f;
        [SerializeField] EaseType easeType = EaseType.InOutSine;

        MeshRenderer _rend;
        Material _mat;
        int _tID;

        void Awake()
        {
            if (!audioHandler) audioHandler = FindAnyObjectByType<AudioHandler>();
            
            _rend = GetComponent<MeshRenderer>();
            if (_rend) _mat = _rend.material;

            _tID = Shader.PropertyToID("_T");
        }

        void Update()
        {
            if (!audioHandler || !_mat || audioHandler.currentPattern == null || audioHandler.currentPattern.Length == 0) return;

            double stepSize = 1.0;
            switch (audioHandler.patternResolution)
            {
                case PatternResolution.Whole: stepSize = 1.0; break;
                case PatternResolution.Half: stepSize = 0.5; break;
                case PatternResolution.Quarter: stepSize = 0.25; break;
            }

            double currentBeat = audioHandler.songPositionInBeats;
            int centerStep = (int)System.Math.Round(currentBeat / stepSize);

            double smallestDiff = double.MaxValue;
            int closestActiveStep = -1;

            for (int i = -2; i <= 2; i++)
            {
                int checkStep = centerStep + i;
                if (IsStepActive(checkStep))
                {
                    double exactStepBeat = checkStep * stepSize;
                    double diff = System.Math.Abs(currentBeat - exactStepBeat);
                    
                    if (diff < smallestDiff)
                    {
                        smallestDiff = diff;
                        closestActiveStep = checkStep;
                    }
                }
            }

            float t = 0f;
            if (closestActiveStep != -1 && smallestDiff <= pulseRadiusInBeats)
            {
                float normalizedDist = (float)(smallestDiff / pulseRadiusInBeats);
                t = 1f - normalizedDist;
                t = Easing.Evaluate(easeType, t);
            }

            _mat.SetFloat(_tID, t);
        }

        bool IsStepActive(int stepIndex)
        {
            int length = audioHandler.currentPattern.Length;
            int mod = stepIndex % length;
            if (mod < 0) mod += length;
            return audioHandler.currentPattern[mod];
        }

        void OnDestroy()
        {
            if (_mat) Destroy(_mat);
        }
    }
}