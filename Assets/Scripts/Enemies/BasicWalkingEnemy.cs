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
        
        [Header("VFX")]
        public GameObject deathParticlePrefab;

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
                // Calculate normalized time (0 to 1) for the stun duration
                float t = Mathf.Clamp01(1f - (_stunTimer / _stunDuration));
                
                // Evaluate the easing curve and invert it so velocity decays from 1 to 0
                float velocityMultiplier = 1f - Easing.Evaluate(knockbackEase, t);
                
                Vector3 currentKnockback = _knockbackVelocity * velocityMultiplier;
                
                // Ensure knockback remains purely 2D if on the XY plane
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
            
            if (ObjectPooler.Instance != null && deathParticlePrefab != null)
            {
                ObjectPooler.Instance.Get(deathParticlePrefab, transform.position, Quaternion.identity);
            }

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