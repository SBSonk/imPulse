using UnityEngine;
using ImPulse.Core;

namespace ImPulse.Enemies
{
    [RequireComponent(typeof(CustomCollider))]
    public abstract class Enemy : HasHealth 
    {
        [Header("Movement Stats")]
        public float moveSpeed = 3f;

        protected CustomCollider _collider;
        protected AudioHandler _audioHandler;
        protected Transform _playerTransform;

        protected override void Awake()
        {
            base.Awake();

            _collider = GetComponent<CustomCollider>();
            _audioHandler = FindAnyObjectByType<AudioHandler>();

            // theres only one player anyway
            if (CustomCollider.LayerGroups[(int)CollisionLayer.Player].Count > 0)
            {
                _playerTransform = CustomCollider.LayerGroups[(int)CollisionLayer.Player][0].transform;
            }
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

        protected virtual void Update()
        {
            if (_playerTransform == null && CustomCollider.LayerGroups[(int)CollisionLayer.Player].Count > 0)
            {
                _playerTransform = CustomCollider.LayerGroups[(int)CollisionLayer.Player][0].transform;
            } 
            else if (CustomCollider.LayerGroups[(int)CollisionLayer.Player].Count <= 0) return;

            HandleMovement();
            HandleAttack();
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
            Destroy(gameObject);
        }
    }
}