using UnityEngine;
using ImPulse.Core;
using ImPulse.Player;
using ImPulse.Core.Pooling;

namespace ImPulse.Collectibles
{
    public abstract class Pickup : CustomCollider, IPoolable
    {
        [Header("Magnet Settings")]
        public float magnetRadius = 3f;
        public float magnetSpeed = 15f;

        protected GameObject _sourcePrefab;
        private Transform _playerTransform;

        protected virtual void Awake()
        {
            layersToCheck = CollisionLayerMask.Player;
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
            if (_playerTransform == null)
            {
                var player = FindAnyObjectByType<PlayerMovement>();
                if (player != null) _playerTransform = player.transform;
            }
        }

        public virtual void OnPoolReturn() { }

        protected override void Update()
        {
            base.Update();

            if (_playerTransform != null)
            {
                float dist = Vector3.Distance(transform.position, _playerTransform.position);
                if (dist <= magnetRadius)
                {
                    transform.position = Vector3.MoveTowards(transform.position, _playerTransform.position, magnetSpeed * Time.deltaTime);
                }
            }
        }

        private void HandleCollisionEnter(CustomCollider other)
        {
            if (other.TryGetComponent<HasHealth>(out var playerHealth))
            {
                Collect(other.gameObject);
                ReturnToPool();
            }
        }

        protected abstract void Collect(GameObject player);

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