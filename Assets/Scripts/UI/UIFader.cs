using UnityEngine;
using ImPulse.Helpers;

namespace ImPulse.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIFader : MonoBehaviour
    {
        [Header("Fade Settings")]
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private EaseType fadeEase = EaseType.OutCubic;

        private CanvasGroup _canvasGroup;
        private bool _isFading = false;
        private float _fadeTimer = 0f;
        private float _startAlpha;
        private float _targetAlpha;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
        }

        private void Update()
        {
            if (!_isFading) return;

            _fadeTimer += Time.unscaledDeltaTime;
            float normTime = Mathf.Clamp01(_fadeTimer / fadeDuration);
            float easeVal = Easing.Evaluate(fadeEase, normTime);

            _canvasGroup.alpha = Mathf.Lerp(_startAlpha, _targetAlpha, easeVal);

            if (_fadeTimer >= fadeDuration)
            {
                _isFading = false;
            }
        }

        public void FadeTo(float targetAlpha)
        {
            _startAlpha = _canvasGroup.alpha;
            _targetAlpha = targetAlpha;
            _fadeTimer = 0f;
            _isFading = true;
            
            _canvasGroup.blocksRaycasts = targetAlpha > 0f;
        }
    }
}