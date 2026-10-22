using System;
using System.Collections.Generic;
using UnityEngine;

namespace ImPulse.Core
{
    public enum RhythmInputType
    {
        None = 0,
        Dash = 1,
        Melee = 2,
        Ranged = 3
    }

    public enum SleightType
    {
        None = 0,
        Infinity = 1
    }

    [Serializable]
    public struct SleightRecipe
    {
        public SleightType sleight;
        public RhythmInputType[] sequence;
    }

    public class RhythmInputHandler : MonoBehaviour
    {
        [Header("References")]
        public AudioHandler audioHandler;

        [Header("Combo Settings")]
        public int maxCombo = 4;
        public int CurrentCombo { get; private set; } = 0;
        
        public bool IsFinisher => CurrentCombo == maxCombo && maxCombo > 0;

        [Header("Sleight Settings")]
        public List<SleightRecipe> sleightRecipes = new List<SleightRecipe>();
        
        private SleightType _activeSleight = SleightType.None;
        public SleightType ActiveSleight 
        { 
            get => _activeSleight;
            set
            {
                if (_activeSleight != value)
                {
                    _activeSleight = value;
                    OnActiveSleightChanged?.Invoke(_activeSleight);
                }
            }
        }
        
        public SleightType TriggeredSleight { get; private set; } = SleightType.None;

        [Header("Timing Settings")]
        public double forgivenessWindow = 0.100;

        public event Action OnComboAdvanced;
        public event Action OnComboDropped;
        public event Action<SleightType> OnActiveSleightChanged;
        public event Action<SleightType> OnSleightTriggered;
        public event Action<int, string> OnInputSuccess;
        public event Action OnInputMiss;

        public IReadOnlyList<RhythmInputType> CurrentInputBuffer => _currentInputBuffer;

        private int _lastHitStepIndex = -1;
        private Dictionary<int, SleightType> _sleightsDictionary = new Dictionary<int, SleightType>();
        private List<RhythmInputType> _currentInputBuffer = new List<RhythmInputType>();

        private void Awake()
        {
            InitializeSleights();
        }

        private void InitializeSleights()
        {
            _sleightsDictionary.Clear();
            foreach (var recipe in sleightRecipes)
            {
                if (recipe.sequence != null && recipe.sequence.Length > 0)
                {
                    int hash = HashSequence(recipe.sequence);
                    if (!_sleightsDictionary.ContainsKey(hash))
                    {
                        _sleightsDictionary.Add(hash, recipe.sleight);
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"[RhythmInputHandler] Duplicate Sleight sequence hash detected for {recipe.sleight}!");
                    }
                }
            }
        }

        public static int HashSequence(IList<RhythmInputType> sequence)
        {
            int hash = 0;
            for (int i = 0; i < sequence.Count; i++)
            {
                hash = (hash * 4) + (int)sequence[i];
            }
            return hash;
        }

        public SleightRecipe GetRecipe(SleightType type)
        {
            foreach (var recipe in sleightRecipes)
            {
                if (recipe.sleight == type) return recipe;
            }
            return default;
        }

        public bool ProcessInput(RhythmInputType inputType)
        {
            TriggeredSleight = SleightType.None;

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
                _currentInputBuffer.Clear();
            }
            
            CurrentCombo++;
            _currentInputBuffer.Add(inputType);
            OnComboAdvanced?.Invoke();

            if (IsFinisher)
            {
                int currentHash = HashSequence(_currentInputBuffer);
                if (_sleightsDictionary.TryGetValue(currentHash, out SleightType matchedSleight))
                {
                    if (matchedSleight == ActiveSleight)
                    {
                        TriggeredSleight = matchedSleight;
                        OnSleightTriggered?.Invoke(matchedSleight);
                    }
                }
            }

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
            TriggeredSleight = SleightType.None;
            _currentInputBuffer.Clear();

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