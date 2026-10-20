using UnityEngine;
using ImPulse.Core;

namespace ImPulse.Player
{
    public class PlayerStatus : HasHealth
    {
        [Header("Iframes")]
        [SerializeField] float dashInvincibilityTime = .25f;

        private Animator _animator;
        private PlayerMovement _playerMovement;
        private PlayerAttack _playerAttack;

        protected override void Awake()
        {
            base.Awake();
            
            _animator = GetComponent<Animator>();
            _playerMovement = GetComponent<PlayerMovement>();
            _playerAttack = GetComponent<PlayerAttack>();
        }

        void Start()
        {
            _playerMovement.OnDashStart += ApplyDashInvincibility;
        }

        void OnDestroy()
        {
            _playerMovement.OnDashStart -= ApplyDashInvincibility;
        }

        void ApplyDashInvincibility()
        {
            GrantInvincibility(dashInvincibilityTime);
        }

        public override bool TakeDamage(float amount)
        {
            bool damageAccepted = base.TakeDamage(amount);

            if (damageAccepted && !_isDead)
            {
                UnityEngine.Debug.Log($"Player took {amount} damage! HP: {currentHealth}/{maxHealth}");
            }

            return damageAccepted;
        }

        protected override void Die()
        {
            if (_isDead) return;
            
            base.Die(); 

            UnityEngine.Debug.Log("Player Died! Game Over.");

            if (_playerMovement != null) _playerMovement.enabled = false;
            if (_playerAttack != null) _playerAttack.enabled = false;
        }
    }
}