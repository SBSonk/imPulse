using UnityEngine;
using ImPulse.Core.Pooling;
using ImPulse.Core;
using ImPulse.Player;
using ImPulse.Helpers;

namespace ImPulse.Enemies
{
    public class DashingEnemy : Enemy
    {
        [Header("Dash Settings")]
        public float dashSpeed = 15f;
        public float dashDuration = 0.3f;
        public EaseType dashEase = EaseType.OutQuad;

        [Header("Rhythm Pattern")]
        public PatternResolution dashResolution = PatternResolution.Whole;
        public bool[] dashPattern = { true, false, false, false }; 

        [Header("Visuals")]
        public SpriteRenderer spriteRenderer;
        
        [Header("VFX")]
        public GameObject deathParticlePrefab;

        private bool _isDashing = false;
        private float _dashTimer = 0f;
        private Vector3 _dashDirection;

        protected override void Awake()
        {
            base.Awake();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        protected override void HandleMovement()
        {
            if (_isDead || _playerTransform == null) return;

            if (_stunTimer > 0f)
            {
                // Instantly cancel any active dash if the enemy gets hit/stunned
                _isDashing = false;

                float t = Mathf.Clamp01(1f - (_stunTimer / _stunDuration));
                float velocityMultiplier = 1f - Easing.Evaluate(knockbackEase, t);
                Vector3 currentKnockback = _knockbackVelocity * velocityMultiplier;
                
                currentKnockback.z = 0; 
                transform.position += currentKnockback * Time.deltaTime;
                return; 
            }

            if (_isDashing)
            {
                _dashTimer += Time.deltaTime;
                float dashNormTime = Mathf.Clamp01(_dashTimer / dashDuration);
                
                float speedMultiplier = 1f - Easing.Evaluate(dashEase, dashNormTime);
                
                transform.position += _dashDirection * (dashSpeed * speedMultiplier) * Time.deltaTime;

                if (_dashTimer >= dashDuration)
                {
                    _isDashing = false;
                }
            }
            else
            {
                // Face the player while idling/waiting for the next beat
                Vector3 lookDir = (_playerTransform.position - transform.position).normalized;
                float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        protected override void OnBeat(int beatIndex)
        {
            base.OnBeat(beatIndex);

            if (_isDead)
            {
                if (ObjectPooler.Instance != null && deathParticlePrefab != null)
                {
                    ObjectPooler.Instance.Get(deathParticlePrefab, transform.position, Quaternion.identity);
                }
                if (_collider != null) _collider.enabled = false;
                base.Die();
                return;
            }

            if (dashResolution == PatternResolution.Whole)
            {
                ProcessDashPattern(beatIndex);
            }
        }

        protected override void OnHalfBeat(int halfBeatIndex)
        {
            base.OnHalfBeat(halfBeatIndex);
            
            if (!_isDead && dashResolution == PatternResolution.Half)
            {
                ProcessDashPattern(halfBeatIndex);
            }
        }

        protected override void OnQuarterBeat(int quarterBeatIndex)
        {
            base.OnQuarterBeat(quarterBeatIndex);
            
            if (!_isDead && dashResolution == PatternResolution.Quarter)
            {
                ProcessDashPattern(quarterBeatIndex);
            }
        }

        private void ProcessDashPattern(int step)
        {
            // Only trigger a new dash if not stunned and not already in the middle of one
            if (_stunTimer <= 0f && !_isDashing && dashPattern != null && dashPattern.Length > 0 && _playerTransform != null)
            {
                int stepIndex = step % dashPattern.Length;
                if (stepIndex < 0) stepIndex += dashPattern.Length;

                if (dashPattern[stepIndex])
                {
                    _isDashing = true;
                    _dashTimer = 0f;
                    _dashDirection = (_playerTransform.position - transform.position).normalized;
                }
            }
        }

        protected override void CollisionEnter(CustomCollider other)
        {
            if (other.layer != CollisionLayer.Player) return;

            bool tookDamage = false;
            if (other.TryGetComponent<PlayerStatus>(out var playerStatus))
            {
                tookDamage = playerStatus.TakeDamage(damage);
            }

            if (!tookDamage) return;

            if (other.TryGetComponent<PlayerMovement>(out var playerMovement))
            {
                var dirToPlayer = other.transform.position - transform.position;
                dirToPlayer.Normalize();
                playerMovement.ApplyKnockback(dirToPlayer, knockbackStrength, stunLength);
            }
        }
    }
}