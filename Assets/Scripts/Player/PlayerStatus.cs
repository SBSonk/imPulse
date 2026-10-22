using UnityEngine;
using ImPulse.Core;

namespace ImPulse.Player
{
    public class PlayerStatus : HasHealth
    {
        [Header("Iframes")]
        [SerializeField] float dashInvincibilityTime = .25f;

        [Header("Dynamic Damage Reduction")]
        [SerializeField, Range(0f, 1f)] float startReductionThreshold = 0.30f;
        [SerializeField, Range(0f, 1f)] float maxReductionThreshold = 0.10f;
        [SerializeField, Range(0f, 1f)] float maxDamageReduction = 0.50f;

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
            if (_playerMovement != null)
            {
                _playerMovement.OnDashStart -= ApplyDashInvincibility;
            }
        }

        void ApplyDashInvincibility()
        {
            GrantInvincibility(dashInvincibilityTime);
        }

        public override bool TakeDamage(float amount)
        {
            float healthRatio = currentHealth / maxHealth;
            
            float reductionT = Mathf.InverseLerp(startReductionThreshold, maxReductionThreshold, healthRatio);
            float damageReduction = Mathf.Lerp(0f, maxDamageReduction, reductionT);
            float finalDamage = amount * (1f - damageReduction);

            bool damageAccepted = base.TakeDamage(finalDamage);

            if (damageAccepted && !_isDead)
            {
                UnityEngine.Debug.Log($"Player took {finalDamage:F1} damage (Reduced by {damageReduction * 100:F0}%). HP: {currentHealth:F1}/{maxHealth}");
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

        // --- UPGRADE API --- //
        public void SetMaxHealth(float newMaxHealth)
        {
            float difference = newMaxHealth - maxHealth;
            maxHealth = newMaxHealth;

            // Automatically heal the player for the exact amount of capacity they just gained
            if (difference > 0)
            {
                currentHealth = Mathf.Clamp(currentHealth + difference, 0, maxHealth);
            }
        }
    }
}