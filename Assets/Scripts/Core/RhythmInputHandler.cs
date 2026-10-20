using System;
using UnityEngine;

namespace ImPulse.Core
{
    public class RhythmInputHandler : MonoBehaviour
    {
        [Header("References")]
        public AudioHandler audioHandler;

        [Header("Timing Settings")]
        public double forgivenessWindow = 0.100;

        // Generic events (Visualizer can still use these for hit feedback)
        public event Action<int, string> OnInputSuccess;
        public event Action OnInputMiss;

        private int _lastHitStepIndex = -1;

        /// <summary>
        /// Evaluates the current input against the metronome.
        /// Returns true if it was a valid hit, false if it missed or was spammed.
        /// </summary>
        public bool ProcessInput()
        {
            if (audioHandler == null || audioHandler.currentPattern == null || audioHandler.currentPattern.Length == 0) 
            {
                OnInputMiss?.Invoke();
                return false;
            }

            double currentPositionSec = audioHandler.songPositionInSeconds;
            double secPerBeat = 60.0 / audioHandler.bpm;

            // Determine the subdivision step size
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

            // Search a small radius to find the absolute closest ACTIVE step
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

            // 1. Check if completely off-beat
            if (bestStep == -1 || smallestDiff > forgivenessWindow)
            {
                OnInputMiss?.Invoke();
                return false;
            }

            // 2. Check if the player is spamming the exact same note
            if (bestStep == _lastHitStepIndex)
            {
                OnInputMiss?.Invoke();
                return false; 
            }

            // 3. Valid Hit successfully registered
            _lastHitStepIndex = bestStep;

            double bestStepTime = bestStep * secPerStep;
            double timingDifference = currentPositionSec - bestStepTime;

            string timingFeedback = "Perfect";
            if (timingDifference < -0.03) timingFeedback = "Early";
            else if (timingDifference > 0.03) timingFeedback = "Late";

            OnInputSuccess?.Invoke(bestStep, timingFeedback);
            return true;
        }

        private bool IsStepActive(int stepIndex)
        {
            int length = audioHandler.currentPattern.Length;
            int mod = stepIndex % length;
            
            // Fix modulo math for negative indices so the pre-song count-in parses correctly
            if (mod < 0) mod += length; 
            
            return audioHandler.currentPattern[mod];
        }
    }
}