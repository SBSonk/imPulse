using UnityEngine;
using ImPulse.Core.Pooling;
using ImPulse.Core;
using ImPulse.Player;
using ImPulse.Helpers;
using ImPulse.Weapons; 

namespace ImPulse.Enemies
{
    public class BasicShooterEnemy : Enemy
    {
        [Header("Shooter Settings")]
        public float minDistance = 5f;
        public float maxDistance = 8f;

        [Header("Weapon Configuration")]
        public Weapon equippedWeapon;
        public Transform bulletOrigin;

        [Header("Rhythm Pattern")]
        public PatternResolution shootResolution = PatternResolution.Whole;
        public bool[] shootPattern = { true, false, true, false };

        [Header("Visuals")]
        public SpriteRenderer spriteRenderer;

        // Changed to protected so child classes can use them
        protected bool _isShooting = false;
        protected int _comboIndex = 0;

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
                float t = Mathf.Clamp01(1f - (_stunTimer / _stunDuration));
                float velocityMultiplier = 1f - Easing.Evaluate(knockbackEase, t);
                Vector3 currentKnockback = _knockbackVelocity * velocityMultiplier;
                
                currentKnockback.z = 0; 
                transform.position += currentKnockback * Time.deltaTime;
                return; 
            }

            float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);

            if (_isShooting && distanceToPlayer > maxDistance)
            {
                _isShooting = false;
                _comboIndex = 0; 
            }
            else if (!_isShooting && distanceToPlayer <= minDistance)
            {
                _isShooting = true;
            }

            if (!_isShooting)
            {
                Vector3 direction = (_playerTransform.position - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;
            }
            else
            {
                Vector3 lookDir = (_playerTransform.position - transform.position).normalized;
                float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        protected override void OnBeat(int beatIndex)
        {
            base.OnBeat(beatIndex);

            if (_isDead) return;

            if (shootResolution == PatternResolution.Whole)
            {
                ProcessShootPattern(beatIndex);
            }
        }

        protected override void OnHalfBeat(int halfBeatIndex)
        {
            base.OnHalfBeat(halfBeatIndex);
            
            if (!_isDead && shootResolution == PatternResolution.Half)
            {
                ProcessShootPattern(halfBeatIndex);
            }
        }

        protected override void OnQuarterBeat(int quarterBeatIndex)
        {
            base.OnQuarterBeat(quarterBeatIndex);
            
            if (!_isDead && shootResolution == PatternResolution.Quarter)
            {
                ProcessShootPattern(quarterBeatIndex);
            }
        }

        // Changed to protected virtual so Symphony can override it
        protected virtual void ProcessShootPattern(int step)
        {
            if (_isShooting && _stunTimer <= 0f && shootPattern != null && shootPattern.Length > 0 && equippedWeapon != null)
            {
                int stepIndex = step % shootPattern.Length;
                if (stepIndex < 0) stepIndex += shootPattern.Length;

                if (shootPattern[stepIndex])
                {
                    equippedWeapon.OnAttack(transform, AttackState.Ranged, _comboIndex, true);
                    _comboIndex++;
                }
            }
        }

        protected override void Die()
        {
            if (_isDead) return;

            if (_collider != null)
            {
                _collider.enabled = false;
            }

            base.Die();
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

        public Transform GetBulletOrigin() => bulletOrigin != null ? bulletOrigin : transform;
    }
}