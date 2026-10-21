using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ImPulse.Core;

namespace ImPulse.UI
{
    public class ComboIndicator : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private ComboBeatUI beatPrefab;
        [SerializeField] private Transform beatsContainer;
        [SerializeField] private float resetDelay = 0.5f;

        private RhythmInputHandler _rhythmInputHandler;
        private List<ComboBeatUI> _spawnedBeats = new List<ComboBeatUI>();
        
        private int _currentUICombo = 0;
        private bool _isResetting = false;

        private void Start()
        {
            _rhythmInputHandler = FindAnyObjectByType<RhythmInputHandler>();

            if (_rhythmInputHandler != null)
            {
                SetMaxCombo(_rhythmInputHandler.maxCombo);
                
                _rhythmInputHandler.OnComboAdvanced += AdvanceCombo;
                _rhythmInputHandler.OnComboDropped += DropCombo;
            }
        }

        private void OnDestroy()
        {
            if (_rhythmInputHandler != null)
            {
                _rhythmInputHandler.OnComboAdvanced -= AdvanceCombo;
                _rhythmInputHandler.OnComboDropped -= DropCombo;
            }
        }

        public void SetMaxCombo(int maxCombo)
        {
            StopAllCoroutines();
            _isResetting = false;
            _currentUICombo = 0;

            foreach (ComboBeatUI beat in _spawnedBeats)
            {
                if (beat != null) Destroy(beat.gameObject);
            }
            _spawnedBeats.Clear();

            for (int i = 0; i < maxCombo; i++)
            {
                ComboBeatUI newBeat = Instantiate(beatPrefab, beatsContainer);
                bool isFinisher = (i == maxCombo - 1);
                newBeat.Initialize(isFinisher);
                _spawnedBeats.Add(newBeat);
            }
        }

        public void AdvanceCombo()
        {
            if (_isResetting || _spawnedBeats.Count == 0 || _currentUICombo >= _spawnedBeats.Count) return;

            _spawnedBeats[_currentUICombo].LightUp();
            _currentUICombo++;

            if (_currentUICombo >= _spawnedBeats.Count)
            {
                StartCoroutine(ResetRoutine());
            }
        }
        
        public void DropCombo()
        {
            if (_isResetting) return;
            
            _currentUICombo = 0;
            for (int i = 0; i < _spawnedBeats.Count; i++)
            {
                _spawnedBeats[i].ResetBeat();
            }
        }

        private IEnumerator ResetRoutine()
        {
            _isResetting = true;
            
            yield return new WaitForSeconds(resetDelay);

            _currentUICombo = 0;
            for (int i = 0; i < _spawnedBeats.Count; i++)
            {
                _spawnedBeats[i].ResetBeat();
            }

            _isResetting = false;
        }
    }
}