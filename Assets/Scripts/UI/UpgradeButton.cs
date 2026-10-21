using System;
using ImPulse.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; // Added for UI text manipulation

namespace ImPulse.UI
{
    public class UpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("Visuals")]
        [SerializeField] EaseType ease = EaseType.InOutQuint;
        [SerializeField] float hoverScaleMultiplier = 1.1f;
        [SerializeField] float maxHoverTime = .25f;

        bool _isMouseOver;
        Vector3 _startLocalScale;
        float _hoverTime = 0;

        RectTransform _rectTransform;

        public Action OnClick;

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _startLocalScale = _rectTransform.localScale;
        }

        /// <summary>
        /// Populates the button's UI text components with the provided upgrade data.
        /// </summary>
        public void SetData(string title, string description)
        {
            if (titleText != null) titleText.text = title;
            if (descriptionText != null) descriptionText.text = description;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isMouseOver = true;
            _hoverTime = 0;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isMouseOver = false;
        }

        void Update()
        {
            if (_isMouseOver)
            {
                _hoverTime += Time.unscaledDeltaTime;
            }
            else
            {
                _hoverTime -= Time.unscaledDeltaTime;
            }

            _hoverTime = Mathf.Clamp(_hoverTime, 0, maxHoverTime);

            Vector3 maxScale = _startLocalScale * hoverScaleMultiplier;
            
            float t = Mathf.InverseLerp(0, maxHoverTime, _hoverTime);
            
            _rectTransform.localScale = Vector3.Lerp(_startLocalScale, maxScale, Easing.Evaluate(ease, t));
        }
    }
}