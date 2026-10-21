using UnityEngine;
using ImPulse.Helpers;

namespace ImPulse.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class MenuPanelLerper : MonoBehaviour
    {
        [Header("Positions")]
        [SerializeField] private Vector3 offPosition;
        [SerializeField] private Vector3 onPosition;

        [Header("Animation Settings")]
        [SerializeField] private float transitionDuration = 0.5f;
        [SerializeField] private EaseType transitionEase = EaseType.OutCubic;

        private RectTransform _rectTransform;
        
        [SerializeField] bool _isOn = false;
        private bool _isAnimating = false;
        
        private float _animationTimer = 0f;
        private Vector3 _startPos;
        private Vector3 _targetPos;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _rectTransform.anchoredPosition = _isOn ? onPosition : offPosition;
        }

        private void Update()
        {
            if (!_isAnimating) return;

            _animationTimer += Time.unscaledDeltaTime; 
            float normTime = Mathf.Clamp01(_animationTimer / transitionDuration);
            float easeVal = Easing.Evaluate(transitionEase, normTime);

            _rectTransform.anchoredPosition = Vector2.Lerp(_startPos, _targetPos, easeVal);

            if (_animationTimer >= transitionDuration)
            {
                _isAnimating = false;
            }
        }

        public void SetState(bool turnOn)
        {
            if (_isOn == turnOn && !_isAnimating) return;

            _isOn = turnOn;
            _startPos = _rectTransform.anchoredPosition;
            _targetPos = _isOn ? onPosition : offPosition;
            
            _animationTimer = 0f;
            _isAnimating = true;
        }

        [ContextMenu("Test Toggle (DO NOT USE IN EDIT)")]
        public void ToggleState()
        {
            SetState(!_isOn);
        }
    }
}