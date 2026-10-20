using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ImPulse.Core;
using ImPulse.Helpers;

namespace ImPulse.Rhythm
{
    public class VisualNode
    {
        public RectTransform rect;
        public Image image;
    }

    public class RhythmVisualizer : MonoBehaviour
    {
        [Header("References")]
        public RectTransform nodeContainer; 
        public GameObject beatNodePrefab;
        public RectTransform hitIndicator;

        [Header("Metronome Settings")]
        public float spacingPerBeat = 400f; 
        public int visibleBeats = 10;
        [Tooltip("When the node is recycled. Make this slightly larger than Fade Distance so nodes don't pop out visibly.")]
        public float beatsPastTarget = 3f;

        [Header("Hit Window Readability")]
        public float hittableWindow = 0.2f; 
        public Color defaultColor = Color.white;
        public Color hittableColor = Color.yellow;

        [Header("Fade Settings")]
        [Tooltip("How many beats away from the center the node becomes completely transparent (alpha 0).")]
        public float fadeDistance = 2.5f;
        [Tooltip("The easing curve applied to the fade. InOut curves create a nice bell-curve shape.")]
        public EaseType fadeEaseType = EaseType.InOutSine;

        [Header("Hit Indicator Pulse")]
        public float pulseScale = 1.5f;
        public float pulseRecoverySpeed = 10f;

        [Header("Node Scaling")]
        public Vector3 wholeBeatScale = Vector3.one;
        public Vector3 halfBeatScale = new Vector3(0.75f, 0.75f, 0.75f);
        public Vector3 quarterBeatScale = new Vector3(0.5f, 0.5f, 0.5f);

        AudioHandler _audioHandler;
        RhythmInputHandler _inputHandler;
        private float _currentBeat;

        // Pooling arrays
        private List<VisualNode> _beatNodes = new List<VisualNode>();
        private List<float> _nodeBeatTargets = new List<float>();

        private bool[] _currentPattern;
        private float _stepSize;
        private PatternResolution _patternResolution;

        private void Awake()
        {
            _audioHandler = FindAnyObjectByType<AudioHandler>();
            _inputHandler = FindAnyObjectByType<RhythmInputHandler>();

            if (_inputHandler != null)
            {
                _inputHandler.OnInputSuccess += HandleInputSuccess;
            }
        }

        private void Start()
        {
            if (_audioHandler != null)
            {
                SetPattern(_audioHandler.currentPattern, _audioHandler.patternResolution);
            }
        }

        private void OnDestroy()
        {
            if (_inputHandler != null)
            {
                _inputHandler.OnInputSuccess -= HandleInputSuccess;
            }
        }

        public void SetPattern(bool[] newPattern, PatternResolution resolution)
        {
            if (newPattern == null || newPattern.Length == 0) return;
            
            _currentPattern = newPattern;
            _patternResolution = resolution;
            
            switch (_patternResolution)
            {
                case PatternResolution.Whole: _stepSize = 1.0f; break;
                case PatternResolution.Half: _stepSize = 0.5f; break;
                case PatternResolution.Quarter: _stepSize = 0.25f; break;
            }
            
            InitializePool();
        }

        private void InitializePool()
        {
            foreach (var node in _beatNodes) 
            {
                if (node != null && node.rect != null) Destroy(node.rect.gameObject);
            }
            
            _beatNodes.Clear();
            _nodeBeatTargets.Clear();

            float target = 0f;
            if (!IsBeatActive(target)) target = GetNextActiveBeat(target);

            for (int i = 0; i < visibleBeats; i++)
            {
                GameObject nodeObj = Instantiate(beatNodePrefab, nodeContainer);
                
                VisualNode vNode = new VisualNode
                {
                    rect = nodeObj.GetComponent<RectTransform>(),
                    image = nodeObj.GetComponent<Image>()
                };

                _beatNodes.Add(vNode);
                _nodeBeatTargets.Add(target); 
                
                vNode.rect.localScale = GetScaleForBeat(target);

                target = GetNextActiveBeat(target);
            }
        }

        private void Update()
        {
            if (hitIndicator != null)
            {
                hitIndicator.localScale = Vector3.Lerp(hitIndicator.localScale, Vector3.one, Time.deltaTime * pulseRecoverySpeed);
            }

            if (_audioHandler == null || _currentPattern == null || _currentPattern.Length == 0) return;

            _currentBeat = (float)_audioHandler.songPositionInBeats;

            for (int i = 0; i < _beatNodes.Count; i++)
            {
                VisualNode node = _beatNodes[i];
                float targetBeat = _nodeBeatTargets[i];

                float beatDifference = targetBeat - _currentBeat;

                // Recycle past nodes
                if (beatDifference < -beatsPastTarget) 
                {
                    _nodeBeatTargets[i] = GetNextActiveBeat(GetHighestBeatTarget());
                    beatDifference = _nodeBeatTargets[i] - _currentBeat;
                    
                    node.rect.localScale = GetScaleForBeat(_nodeBeatTargets[i]);
                }

                // Move the node
                float xPos = beatDifference * spacingPerBeat;
                node.rect.anchoredPosition = new Vector2(xPos, 0f);

                if (node.image != null)
                {
                    // 1. Determine base color (Hit window feedback)
                    Color targetColor = Mathf.Abs(beatDifference) <= hittableWindow ? hittableColor : defaultColor;

                    // 2. Calculate Bell Curve Fade
                    float absDiff = Mathf.Abs(beatDifference);
                    // Map distance into 0 to 1 range (0 = at center, 1 = at max fade distance)
                    float normalizedDist = Mathf.Clamp01(absDiff / fadeDistance);
                    
                    // Evaluate ease: dist 0 -> ease 0 (alpha 100%), dist 1 -> ease 1 (alpha 0%)
                    float easeValue = Easing.Evaluate(fadeEaseType, normalizedDist);
                    float alpha = 1f - easeValue;

                    // Apply the alpha while preserving the base color RGB
                    targetColor.a *= alpha; 
                    node.image.color = targetColor;
                }
            }
        }

        private void HandleInputSuccess(int beatIndex, string feedback)
        {
            if (hitIndicator != null)
            {
                hitIndicator.localScale = Vector3.one * pulseScale;
            }
        }

        private float GetHighestBeatTarget()
        {
            float highest = -1f;
            for (int i = 0; i < _nodeBeatTargets.Count; i++)
            {
                if (_nodeBeatTargets[i] > highest) 
                {
                    highest = _nodeBeatTargets[i];
                }
            }
            return highest;
        }

        private float GetNextActiveBeat(float currentBeat)
        {
            float checkBeat = currentBeat + _stepSize;
            
            int safety = 0; 
            while (!IsBeatActive(checkBeat) && safety < 1000)
            {
                checkBeat += _stepSize;
                safety++;
            }
            
            return checkBeat;
        }

        private bool IsBeatActive(float beat)
        {
            int stepIndex = Mathf.RoundToInt(beat / _stepSize);
            if (stepIndex < 0) stepIndex = 0; 
            
            return _currentPattern[stepIndex % _currentPattern.Length];
        }

        private Vector3 GetScaleForBeat(float beat)
        {
            int quarterIndex = Mathf.RoundToInt(beat * 4f) % 4;

            if (quarterIndex == 0) return wholeBeatScale;       
            if (quarterIndex == 2) return halfBeatScale;        
            return quarterBeatScale;                            
        }
    }
}