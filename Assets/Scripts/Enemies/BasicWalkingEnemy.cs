using UnityEngine;
using ImPulse.Core.Pooling;
using ImPulse.Core;
using ImPulse.Player;

namespace ImPulse.Enemies
{
    public class BasicWalkingEnemy : Enemy
    {
        [Header("Visuals")]
        public SpriteRenderer spriteRenderer;
        public Color baseColor = Color.white;
        public Color deathColor = Color.gray;
        
        [Header("VFX")]
        public GameObject deathParticlePrefab;

        private bool _isDead = false;

        protected override void Awake()
        {
            base.Awake();
            
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null) spriteRenderer.color = baseColor;
        }

        void OnValidate()
        {
            GetComponentInChildren<SpriteRenderer>().color = baseColor;
        }

        protected override void HandleMovement()
        {
            if (_isDead || _playerTransform == null) return;

            Vector3 direction = (_playerTransform.position - transform.position).normalized;
            
            transform.position += direction * moveSpeed * Time.deltaTime;
        }

        protected override void Die()
        {
            if (_isDead) return;
            
            _isDead = true;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = deathColor;
            }

            if (_collider != null)
            {
                _collider.enabled = false;
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
                
                base.Die(); 
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