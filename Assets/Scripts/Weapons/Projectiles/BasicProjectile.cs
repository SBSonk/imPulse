using UnityEngine;
using ImPulse.Core;
using ImPulse.Core.Pooling;
using ImPulse.Player;
using ImPulse.Enemies;

namespace ImPulse.Weapons
{
    public class BasicProjectile : Projectile
    {
        [Header("Effects")]
        public GameObject hitVFXPrefab;

        protected override void OnHit(CustomCollider other)
        {
            HasHealth targetHealth = other.GetComponent<HasHealth>();
            
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(_damage);

                
                    Vector3 knockbackDir = targetHealth.transform.position - transform.position;
                    knockbackDir.Normalize();

                    if (targetHealth is PlayerStatus)
                    {
                        if (targetHealth.TryGetComponent<PlayerMovement>(out var movement)) movement.ApplyKnockback(knockbackDir, _knockbackStrength, _stunLength);
                    }

                    if (targetHealth is Enemy)
                    {
                        var enemy = targetHealth as Enemy;
                        enemy.ApplyKnockback(knockbackDir, _knockbackStrength, _stunLength);
                    }
                    
            }

            if (hitVFXPrefab != null && ObjectPooler.Instance != null)
            {
                ObjectPooler.Instance.Get(hitVFXPrefab, transform.position, Quaternion.identity);
            }

            ReturnToPool();
        }
    }
}