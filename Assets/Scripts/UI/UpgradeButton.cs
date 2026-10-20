using System;
using ImPulse.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ImPulse.UI
{
    public class UpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
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
                _hoverTime += Time.deltaTime;
            }
            else
            {
                _hoverTime -= Time.deltaTime;
            }

            _hoverTime = Mathf.Clamp(_hoverTime, 0, maxHoverTime);

            Vector3 maxScale = _startLocalScale * hoverScaleMultiplier;
            
            float t = Mathf.InverseLerp(0, maxHoverTime, _hoverTime);
            
            _rectTransform.localScale = Vector3.Lerp(_startLocalScale, maxScale, Easing.Evaluate(ease, t));
        }
    }

}
