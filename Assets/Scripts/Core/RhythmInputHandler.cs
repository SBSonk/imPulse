using System;
using UnityEngine;

namespace ImPulse.Core
{
    public class RhythmInputHandler : MonoBehaviour
    {
        [Header("References")]
        public AudioHandler audioHandler;

        [Header("Combo Settings")]
        public int maxCombo = 4;
        public int CurrentCombo { get; private set; } = 0;
        
        // Expose the finisher state so other scripts don't have to calculate it
        public bool IsFinisher => CurrentCombo == maxCombo && maxCombo > 0;

        [Header("Timing Settings")]
        public double forgivenessWindow = 0.100;

        // Combo Events for UI
        public event Action OnComboAdvanced;
        public event Action OnComboDropped;

        // Generic events
        public event Action<int, string> OnInputSuccess;
        public event Action OnInputMiss;

        private int _lastHitStepIndex = -1;

        public bool ProcessInput()
        {
            if (audioHandler == null || audioHandler.currentPattern == null || audioHandler.currentPattern.Length == 0) 
            {
                TriggerMiss();
                return false;
            }

            double currentPositionSec = audioHandler.songPositionInSeconds;
            double secPerBeat = 60.0 / audioHandler.bpm;

            double stepSize = 1.0;
            switch (audioHandler.patternResolution)
            {
                case PatternResolution.Whole: stepSize = 1.0; break;
                case PatternResolution.Half: stepSize = 0.5; break;
                case PatternResolution.Quarter: stepSize = 0.25; break;
            }

            double secPerStep = secPerBeat * stepSize;
            int centerStep = (int)Math.Round(currentPositionSec / secPerStep);

            int bestStep = -1;
            double smallestDiff = double.MaxValue;

            for (int i = -2; i <= 2; i++)
            {
                int checkStep = centerStep + i;
                
                if (IsStepActive(checkStep))
                {
                    double exactStepTime = checkStep * secPerStep;
                    double diff = Math.Abs(currentPositionSec - exactStepTime);
                    
                    if (diff < smallestDiff)
                    {
                        smallestDiff = diff;
                        bestStep = checkStep;
                    }
                }
            }

            if (bestStep == -1 || smallestDiff > forgivenessWindow)
            {
                TriggerMiss();
                return false;
            }

            if (bestStep == _lastHitStepIndex)
            {
                TriggerMiss();
                return false; 
            }

            _lastHitStepIndex = bestStep;

            if (CurrentCombo >= maxCombo)
            {
                CurrentCombo = 0; 
            }
            
            CurrentCombo++;
            OnComboAdvanced?.Invoke();

            double bestStepTime = bestStep * secPerStep;
            double timingDifference = currentPositionSec - bestStepTime;

            string timingFeedback = "Perfect";
            if (timingDifference < -0.03) timingFeedback = "Early";
            else if (timingDifference > 0.03) timingFeedback = "Late";

            OnInputSuccess?.Invoke(bestStep, timingFeedback);
            return true;
        }

        private void TriggerMiss()
        {
            DropCombo();
            OnInputMiss?.Invoke();
        }

        public void DropCombo()
        {
            if (CurrentCombo > 0)
            {
                CurrentCombo = 0;
                OnComboDropped?.Invoke();
            }
        }

        private bool IsStepActive(int stepIndex)
        {
            int length = audioHandler.currentPattern.Length;
            int mod = stepIndex % length;
            if (mod < 0) mod += length; 
            return audioHandler.currentPattern[mod];
        }
    }
}