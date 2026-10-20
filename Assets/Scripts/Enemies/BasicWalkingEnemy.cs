using UnityEngine;
using ImPulse.Core.Pooling;

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
    }
}