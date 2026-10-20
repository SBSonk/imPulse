using UnityEngine;
using ImPulse.Core;
using ImPulse.Core.Pooling;

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
            }

            if (hitVFXPrefab != null && ObjectPooler.Instance != null)
            {
                ObjectPooler.Instance.Get(hitVFXPrefab, transform.position, Quaternion.identity);
            }

            ReturnToPool();
        }
    }
}