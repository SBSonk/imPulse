using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ImPulse.Core;
using TMPro;

namespace ImPulse.UI
{
    [System.Serializable]
    public struct InputSpriteMapping
    {
        public RhythmInputType inputType;
        public Sprite iconSprite;
    }

    public class SleightIndicatorUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MenuPanelLerper panelLerper;
        [SerializeField] private TextMeshProUGUI sleightNameText;
        [SerializeField] private Transform iconsContainer;
        [SerializeField] private SleightInputIconUI iconPrefab;

        [Header("Settings")]
        [SerializeField] private List<InputSpriteMapping> inputSprites;
        [SerializeField] private float successDisplayDuration = 0.5f;
        [Tooltip("How long to wait before wiping incorrect inputs after finishing a combo.")]
        [SerializeField] private float resetDelay = 0.5f;

        private RhythmInputHandler _rhythmHandler;
        private List<SleightInputIconUI> _spawnedIcons = new List<SleightInputIconUI>();
        private SleightRecipe _currentRecipe;
        private bool _isFlashingSuccess = false;
        
        private SleightType _pendingSleight = SleightType.None;
        private Coroutine _delayedResetCoroutine;

        private void Start()
        {
            _rhythmHandler = FindAnyObjectByType<RhythmInputHandler>();
            
            if (_rhythmHandler != null)
            {
                _rhythmHandler.OnActiveSleightChanged += HandleSleightChanged;
                _rhythmHandler.OnComboAdvanced += HandleComboAdvanced;
                _rhythmHandler.OnComboDropped += HandleComboDropped;
                _rhythmHandler.OnSleightTriggered += HandleSleightTriggered;

                HandleSleightChanged(_rhythmHandler.ActiveSleight);
            }
        }

        private void OnDestroy()
        {
            if (_rhythmHandler != null)
            {
                _rhythmHandler.OnActiveSleightChanged -= HandleSleightChanged;
                _rhythmHandler.OnComboAdvanced -= HandleComboAdvanced;
                _rhythmHandler.OnComboDropped -= HandleComboDropped;
                _rhythmHandler.OnSleightTriggered -= HandleSleightTriggered;
            }
        }

        private void HandleSleightChanged(SleightType newSleight)
        {
            if (newSleight == SleightType.None)
            {
                _pendingSleight = SleightType.None;
                
                if (!_isFlashingSuccess && panelLerper != null)
                {
                    panelLerper.SetState(false);
                }
            }
            else
            {
                // Safe to show instantly if the combo is 0 OR if we are in the post-finisher idle state
                if (_rhythmHandler.CurrentCombo == 0 || _rhythmHandler.CurrentCombo >= _rhythmHandler.maxCombo)
                {
                    ShowSleight(newSleight);
                }
                else
                {
                    _pendingSleight = newSleight;
                }
            }
        }

        private void ShowSleight(SleightType sleight)
        {
            SetupSleight(sleight);
            if (panelLerper != null) panelLerper.SetState(true);
        }

        private void SetupSleight(SleightType sleight)
        {
            _currentRecipe = _rhythmHandler.GetRecipe(sleight);
            
            if (sleightNameText != null)
            {
                sleightNameText.text = sleight.ToString();
            }

            foreach (var icon in _spawnedIcons)
            {
                if (icon != null) Destroy(icon.gameObject);
            }
            _spawnedIcons.Clear();

            if (_currentRecipe.sequence == null) return;

            foreach (var inputType in _currentRecipe.sequence)
            {
                SleightInputIconUI newIcon = Instantiate(iconPrefab, iconsContainer);
                newIcon.Initialize(GetSpriteForInput(inputType));
                _spawnedIcons.Add(newIcon);
            }
        }

        private void HandleComboAdvanced()
        {
            if (_delayedResetCoroutine != null)
            {
                StopCoroutine(_delayedResetCoroutine);
                _delayedResetCoroutine = null;
            }

            if (_pendingSleight != SleightType.None && _rhythmHandler.CurrentCombo == 1)
            {
                ShowSleight(_pendingSleight);
                _pendingSleight = SleightType.None;
            }

            if (_rhythmHandler.CurrentCombo == 1 && !_isFlashingSuccess)
            {
                foreach (var icon in _spawnedIcons) icon.ResetIcon();
            }

            if (_rhythmHandler.ActiveSleight == SleightType.None || _currentRecipe.sequence == null) return;

            var buffer = _rhythmHandler.CurrentInputBuffer;
            int currentIndex = buffer.Count - 1;

            if (currentIndex >= 0 && currentIndex < _spawnedIcons.Count)
            {
                if (buffer[currentIndex] == _currentRecipe.sequence[currentIndex])
                {
                    _spawnedIcons[currentIndex].SetCorrect();
                }
                else
                {
                    _spawnedIcons[currentIndex].SetIncorrect();
                }
            }

            if (_rhythmHandler.IsFinisher)
            {
                _delayedResetCoroutine = StartCoroutine(DelayedWipeRoutine());
            }
        }

        private void HandleComboDropped()
        {
            foreach (var icon in _spawnedIcons)
            {
                icon.ResetIcon();
            }

            if (_pendingSleight != SleightType.None)
            {
                ShowSleight(_pendingSleight);
                _pendingSleight = SleightType.None;
            }
        }

        private void HandleSleightTriggered(SleightType triggeredSleight)
        {
            if (triggeredSleight == _currentRecipe.sleight)
            {
                StartCoroutine(SuccessRoutine());
            }
        }

        private IEnumerator SuccessRoutine()
        {
            _isFlashingSuccess = true;

            foreach (var icon in _spawnedIcons)
            {
                icon.SetSuccess();
            }

            yield return new WaitForSeconds(successDisplayDuration);

            if (panelLerper != null)
            {
                panelLerper.SetState(false);
            }

            _isFlashingSuccess = false;
        }

        private IEnumerator DelayedWipeRoutine()
        {
            yield return new WaitForSeconds(resetDelay);

            if (!_isFlashingSuccess)
            {
                foreach (var icon in _spawnedIcons)
                {
                    icon.ResetIcon();
                }
                
                // If a new sleight is waiting, show it now that the previous sequence has visually concluded
                if (_pendingSleight != SleightType.None)
                {
                    ShowSleight(_pendingSleight);
                    _pendingSleight = SleightType.None;
                }
            }
        }

        private Sprite GetSpriteForInput(RhythmInputType type)
        {
            foreach (var mapping in inputSprites)
            {
                if (mapping.inputType == type) return mapping.iconSprite;
            }
            return null;
        }
    }
}