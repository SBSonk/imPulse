using System;
using UnityEngine;

namespace ImPulse.Core
{
    public class HasHealth : MonoBehaviour
    {
        [Header("Health Stats")]
        public float maxHealth = 50f;
        public float currentHealth { get; protected set; }
        
        [Tooltip("How long the entity is invincible after taking damage.")]
        public float hitInvincibilityDuration = 0f;

        public event Action<float, float> OnHealthChanged;
        public event Action OnDeath;

        protected bool _isDead = false;
        protected float _invincibilityTimer = 0f;

        protected virtual void Awake()
        {
            currentHealth = maxHealth;
        }

        protected virtual void Update()
        {
            if (_invincibilityTimer > 0f)
            {
                _invincibilityTimer -= Time.deltaTime;
            }
        }

        public virtual bool TakeDamage(float amount)
        {
            if (_isDead || _invincibilityTimer > 0f) return false;

            currentHealth -= amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0f)
            {
                Die();
            }
            else if (hitInvincibilityDuration > 0f)
            {
                GrantInvincibility(hitInvincibilityDuration);
            }

            return true;
        }

        public virtual bool Heal(float amount)
        {
            if (_isDead || currentHealth >= maxHealth) return false;

            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            return true;
        }

        public void GrantInvincibility(float duration)
        {
            // Only overwrite if the new duration is longer than the remaining time
            if (duration > _invincibilityTimer)
            {
                _invincibilityTimer = duration;
            }
        }

        protected virtual void Die()
        {
            if (_isDead) return;

            _isDead = true;
            OnDeath?.Invoke();
        }
    }
}