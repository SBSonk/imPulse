using ImPulse.Core;
using ImPulse.Helpers;
using ImPulse.Player;
using UnityEngine;

namespace ImPulse.UI
{
    public class WorldSpaceRhythmUI : MonoBehaviour
    {
        [Header("Rhythm Pulsing")]
        [SerializeField] EaseType pulseEase = EaseType.OutQuart;
        [SerializeField] float pulseTime = .5f;
        [SerializeField] float fullPulseScaleMultiplier = 1.25f, halfPulseScaleMultiplier = 1.1f, quarterPulseScaleMultiplier = 1.05f;

        [Header("Look Ahead")]
        [SerializeField] float followDamp = 1;
        [SerializeField] Vector2 maxOffsetPerAxis = Vector2.one;
        [SerializeField] float lookAheadTime = .1f;

        AudioHandler _audioHandler;
        PlayerMovement _playerMovement;
        Vector3 _startLocalPosition;
        float _currentPulseScale = 1;
        Vector3 _dampVel;

        Vector3 _startScale;

        float _pulseT = 0;

        void Awake()
        {
            _playerMovement = FindAnyObjectByType<PlayerMovement>();
            _audioHandler = FindAnyObjectByType<AudioHandler>();
        }

        void Start()
        {
            _startLocalPosition = transform.localPosition;
            _startScale = transform.localScale;

            _audioHandler.OnWholePatternBeat += PulseUIWhole;
            _audioHandler.OnHalfPatternBeat += PulseUIHalf;
            _audioHandler.OnQuarterPatternBeat += PulseUIQuarter;
        }

        void OnDestroy()
        {
            _audioHandler.OnWholePatternBeat -= PulseUIWhole;
            _audioHandler.OnHalfPatternBeat -= PulseUIHalf;
            _audioHandler.OnQuarterPatternBeat -= PulseUIQuarter;
        }

        void Update()
        {
            HandleSway();
            HandlePulsing();
        }

        void HandleSway()
        {
            Vector3 pVel = _playerMovement.GetVelocity();
            Vector3 offset = (pVel.normalized * pVel.magnitude * Time.deltaTime) * lookAheadTime;

            offset.x = Mathf.Clamp(offset.x, -maxOffsetPerAxis.x, maxOffsetPerAxis.x);
            offset.y = Mathf.Clamp(offset.y, -maxOffsetPerAxis.y, maxOffsetPerAxis.y);

            Vector3 targetPos = _startLocalPosition - offset;
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPos, ref _dampVel, followDamp);
        }

        void HandlePulsing()
        {
            Vector3 scaledScale = _startScale * _currentPulseScale;

            transform.localScale = Vector3.Lerp(_startScale, scaledScale, Easing.Evaluate(pulseEase, _pulseT));

            _pulseT -= Time.deltaTime;
            _pulseT = Mathf.Clamp01(_pulseT);
        }

        void PulseUIWhole(int _)
        {
            _pulseT = pulseTime;
            _currentPulseScale = fullPulseScaleMultiplier;
        }

        void PulseUIHalf(int _)
        {
            _pulseT = pulseTime;
            _currentPulseScale = halfPulseScaleMultiplier;
        }

        void PulseUIQuarter(int _)
        {
            _pulseT = pulseTime;
            _currentPulseScale = quarterPulseScaleMultiplier;
        }
    }
}