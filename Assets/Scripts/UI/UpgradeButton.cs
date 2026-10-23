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

        [Header("Audio")]
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip selectSound;
        [Tooltip("The AudioSource used to play the UI sounds. Needs to ignore TimeScale.")]
        AudioSource _audioSource;

        bool _isMouseOver;
        Vector3 _startLocalScale;
        float _hoverTime = 0;

        RectTransform _rectTransform;

        public Action OnClick;

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _startLocalScale = _rectTransform.localScale;

            if (!TryGetComponent<AudioSource>(out var _)) gameObject.AddComponent<AudioSource>();
            _audioSource = GetComponent<AudioSource>();

            _audioSource.loop = false;
            _audioSource.playOnAwake = false;

        }

        public void SetData(string title, string description)
        {
            if (titleText != null) titleText.text = title;
            if (descriptionText != null) descriptionText.text = description;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isMouseOver = true;
            _hoverTime = 0;

            if (_audioSource != null && hoverSound != null)
            {
                _audioSource.PlayOneShot(hoverSound);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_audioSource != null && selectSound != null)
            {
                _audioSource.PlayOneShot(selectSound);
            }

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