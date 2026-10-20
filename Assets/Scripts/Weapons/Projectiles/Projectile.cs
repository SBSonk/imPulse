using UnityEngine;
using ImPulse.Core;
using ImPulse.Core.Pooling;

namespace ImPulse.Weapons
{
    public abstract class Projectile : CustomCollider, IPoolable
    {
        protected float _speed;
        protected float _damage;
        protected float _maxLifetime;

        protected GameObject _sourcePrefab;
        protected Vector3 _direction;
        protected float _lifetimeTimer;
        
        protected TrailRenderer _trailRenderer;

        protected virtual void Awake()
        {
            _trailRenderer = GetComponentInChildren<TrailRenderer>();
        }

        protected virtual void OnEnable()
        {
            this.CollisionEnter += HandleCollisionEnter;
        }

        protected virtual void OnDisable()
        {
            this.CollisionEnter -= HandleCollisionEnter;
        }

        public virtual void OnPoolGet(GameObject sourcePrefab)
        {
            _sourcePrefab = sourcePrefab;
        }

        public virtual void OnPoolReturn()
        {
            _direction = Vector3.zero;
            
            if (_trailRenderer != null)
            {
                _trailRenderer.Clear();
                _trailRenderer.enabled = false;
            }
        }

        public virtual void InitializeProjectile(float speed, float damage, float maxLifetime)
        {
            _speed = speed;
            _damage = damage;
            _maxLifetime = maxLifetime;
            _lifetimeTimer = _maxLifetime;
        }

        public virtual void Fire(Vector3 direction)
        {
            _direction = direction.normalized;
            if (_direction != Vector3.zero)
            {
                float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            if (_trailRenderer != null)
            {
                _trailRenderer.Clear();
                _trailRenderer.enabled = true;
            }
        }

        protected override void Update()
        {
            base.Update(); // run the collisionc heck

            _lifetimeTimer -= Time.deltaTime;
            if (_lifetimeTimer <= 0f)
            {
                ReturnToPool();
                return;
            }

            HandleMovement();
        }

        protected virtual void HandleMovement()
        {
            Vector3 moveDelta = _direction * _speed * Time.deltaTime;
            moveDelta.z = 0; 
            
            transform.position += moveDelta;
        }

        private void HandleCollisionEnter(CustomCollider other)
        {
            OnHit(other);
        }

        protected abstract void OnHit(CustomCollider other);

        protected virtual void ReturnToPool()
        {
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