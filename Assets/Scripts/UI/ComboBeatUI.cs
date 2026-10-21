using UnityEngine;
using UnityEngine.UI;
using ImPulse.Helpers;

namespace ImPulse.UI
{
    public class ComboBeatUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image beatImage;

        [Header("Colors (Cyber Yellow Theme)")]
        [SerializeField] private Color unlitColor = new Color(0.2f, 0.165f, 0f, 1f); 
        [SerializeField] private Color litColor = new Color(1f, 0.827f, 0f, 1f);

        [Header("Scale Settings")]
        [SerializeField] private Vector3 normalScale = Vector3.one;
        [SerializeField] private Vector3 finisherScale = new Vector3(1.5f, 1.5f, 1.5f);
        
        [Header("Animation")]
        [SerializeField] private float animDuration = 0.25f;
        [Tooltip("OutBack gives it a nice snappy 'pop' effect.")]
        [SerializeField] private EaseType easeType = EaseType.OutBack; 

        private bool _isLit;
        private bool _isFinisher;
        private float _animTimer;
        
        private Color _startColor;
        private Vector3 _startScale;

        public void Initialize(bool isFinisher)
        {
            _isFinisher = isFinisher;
            ResetBeat(instant: true);
        }

        public void LightUp()
        {
            if (_isLit) return;
            
            _isLit = true;
            _animTimer = 0f;
            _startColor = beatImage.color;
            _startScale = transform.localScale;
        }

        public void ResetBeat(bool instant = false)
        {
            _isLit = false;
            Vector3 targetScale = _isFinisher ? finisherScale : normalScale;

            if (instant)
            {
                if (beatImage != null) beatImage.color = unlitColor;
                transform.localScale = targetScale;
                _animTimer = animDuration; // Prevent update from running
            }
            else
            {
                _animTimer = 0f;
                _startColor = beatImage.color;
                _startScale = transform.localScale;
            }
        }

        private void Update()
        {
            if (_animTimer < animDuration)
            {
                _animTimer += Time.deltaTime;
                float t = Mathf.Clamp01(_animTimer / animDuration);
                float easeVal = Easing.Evaluate(easeType, t);

                Color targetColor = _isLit ? litColor : unlitColor;
                Vector3 targetScale = _isFinisher ? finisherScale : normalScale;
                
                if (beatImage != null)
                {
                    beatImage.color = Color.Lerp(_startColor, targetColor, easeVal);
                }
                
                transform.localScale = Vector3.Lerp(_startScale, targetScale, easeVal);
            }
        }
    }
}