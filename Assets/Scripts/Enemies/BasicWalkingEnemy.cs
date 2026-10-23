using UnityEngine;
using ImPulse.Core.Pooling;
using ImPulse.Core;
using ImPulse.Player;
using ImPulse.Helpers;

namespace ImPulse.Enemies
{
    public class BasicWalkingEnemy : Enemy
    {
        [Header("Visuals")]
        public SpriteRenderer spriteRenderer;

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
            }
            else
            {
                // Regain standard movement once the stun timer reaches 0
                Vector3 direction = (_playerTransform.position - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;
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
    }
}