using UnityEngine;
using ImPulse.Helpers;

namespace ImPulse.UI
{
    public class MaskedBar : MonoBehaviour
    {
        [Header("Bar References")]
        [SerializeField] private RectTransform mainBar;
        [SerializeField] private RectTransform ghostBar;

        [Header("Bar Settings")]
        [SerializeField] private float barWidth = 100f;
        
        [Header("Main Animation")]
        [SerializeField] private float mainDuration = 0.2f;
        [SerializeField] private EaseType mainEaseType = EaseType.OutQuad;

        private float _targetT = 1f;
        
        private float _currentMainT = 1f;
        private float _startMainT = 1f;
        private float _mainTimer = 0f;

        private float _startPosX;

        private void Awake()
        {
            if (mainBar != null)
            {
                _startPosX = mainBar.anchoredPosition.x;
                
                if (barWidth <= 0)
                {
                    barWidth = mainBar.rect.width;
                }
            }
        }

        private void Update()
        {
            if (mainBar == null) return;

            if (_mainTimer < mainDuration)
            {
                _mainTimer += Time.deltaTime;
                float normTime = Mathf.Clamp01(_mainTimer / mainDuration);
                float easeVal = Easing.Evaluate(mainEaseType, normTime);
                
                _currentMainT = Mathf.Lerp(_startMainT, _targetT, easeVal);
                UpdateBarPosition(mainBar, _currentMainT);
            }
        }

        public void SetFill(float t)
        {
            t = Mathf.Clamp01(t);

            _startMainT = _currentMainT;
            _mainTimer = 0f;

            _targetT = t;
        }

        public void SetFillInstant(float t)
        {
            t = Mathf.Clamp01(t);
            _targetT = t;
            
            _currentMainT = t;
            _startMainT = t;
            _mainTimer = mainDuration;
        

            UpdateBarPosition(mainBar, _currentMainT);
        }

        private void UpdateBarPosition(RectTransform bar, float tValue)
        {
            float targetX = Mathf.Lerp(_startPosX - barWidth, _startPosX, tValue);
            
            Vector2 newPos = bar.anchoredPosition;
            newPos.x = targetX;
            bar.anchoredPosition = newPos;
        }
    }
}