using UnityEngine;
using ImPulse.Core;
using ImPulse.Helpers;
using ImPulse.Core.Pooling;
using System;

namespace ImPulse.Enemies
{
    [RequireComponent(typeof(CustomCollider))]
    public abstract class Enemy : HasHealth, IPoolable 
    {
        public event Action<Enemy, int> OnEnemyDied;

        [Header("Attack Stats")]
        [SerializeField] protected float damage = 10;
        [SerializeField] protected float knockbackStrength = 5;
        [SerializeField] protected float stunLength = .5f;

        [Header("Movement Stats")]
        [SerializeField] protected float moveSpeed = 3f;

        [Header("Hit Reaction Settings")]
        [SerializeField] protected EaseType knockbackEase = EaseType.OutQuad;
        
        protected Vector3 _knockbackVelocity;
        protected float _stunTimer = 0f;
        protected float _stunDuration = 0f;

        protected CustomCollider _collider;
        protected AudioHandler _audioHandler;
        protected Transform _playerTransform;
        
        protected GameObject _sourcePrefab;
        private int _xpYield = 0;

        protected override void Awake()
        {
            base.Awake();

            _collider = GetComponent<CustomCollider>();
            _audioHandler = FindAnyObjectByType<AudioHandler>();

            if (CustomCollider.LayerGroups[(int)CollisionLayer.Player].Count > 0)
            {
                _playerTransform = CustomCollider.LayerGroups[(int)CollisionLayer.Player][0].transform;
            }
        }

        public void InitializeWaveStats(float healthMultiplier, int xpYield)
        {
            maxHealth *= healthMultiplier;
            currentHealth = maxHealth;
            _xpYield = xpYield;
        }
        
        public virtual void OnPoolGet(GameObject sourcePrefab)
        {
            _sourcePrefab = sourcePrefab;
        }

        public virtual void OnPoolReturn()
        {
            _knockbackVelocity = Vector3.zero;
            _stunTimer = 0f;
        }

        protected virtual void OnEnable()
        {
            if (_collider != null)
            {
                _collider.CollisionEnter += CollisionEnter;
                _collider.CollisionStay += CollisionStay;
                _collider.CollisionExit += CollisionExit;
            }

            if (_audioHandler != null)
            {
                _audioHandler.OnBeat += OnBeat;
                _audioHandler.OnHalfBeat += OnHalfBeat;
            }
            
            _isDead = false;
            if (_collider != null) _collider.enabled = true;
        }

        protected virtual void OnDisable()
        {
            if (_collider != null)
            {
                _collider.CollisionEnter -= CollisionEnter;
                _collider.CollisionStay -= CollisionStay;
                _collider.CollisionExit -= CollisionExit;
            }

            if (_audioHandler != null)
            {
                _audioHandler.OnBeat -= OnBeat;
                _audioHandler.OnHalfBeat -= OnHalfBeat;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (_stunTimer > 0f)
            {
                _stunTimer -= Time.deltaTime;
            }

            if (_playerTransform == null && CustomCollider.LayerGroups[(int)CollisionLayer.Player].Count > 0)
            {
                _playerTransform = CustomCollider.LayerGroups[(int)CollisionLayer.Player][0].transform;
            } 
            else if (CustomCollider.LayerGroups[(int)CollisionLayer.Player].Count <= 0) return;

            HandleMovement();
            HandleAttack();
        }

        public virtual void ApplyKnockback(Vector3 direction, float force, float duration)
        {
            _knockbackVelocity = direction.normalized * force;
            _stunDuration = duration;
            _stunTimer = duration;
        }

        protected virtual void HandleMovement() { }
        protected virtual void HandleAttack() { }
        
        protected virtual void OnBeat(int beatIndex) { }
        protected virtual void OnHalfBeat(int halfBeatIndex) { }

        protected virtual void CollisionEnter(CustomCollider other) { }
        protected virtual void CollisionStay(CustomCollider other) { }
        protected virtual void CollisionExit(CustomCollider other) { }

        protected override void Die()
        {
            if (_isDead) return;
            
            base.Die(); 
            
            OnEnemyDied?.Invoke(this, _xpYield);
            
            if (ObjectPooler.Instance != null && _sourcePrefab != null)
            {
                ObjectPooler.Instance.Return(_sourcePrefab, gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}