using System.Collections;
using UnityEngine;
using ImPulse.Player;
using ImPulse.Core.Pooling;

namespace ImPulse.Weapons
{
    [CreateAssetMenu(fileName = "Infinity Sleight Weapon", menuName = "ImPulse/Weapons/Sleights/Infinity")]
    public class InfinitySleightWeapon : RangedWeapon
    {
        [Header("Infinity Sleight Settings")]
        public int bulletCount = 12;
        public float spawnRadiusOffset = 0.75f;
        public float timeBetweenShots = 0.05f;

        public override void OnAttack(Transform playerTransform, AttackState state, int comboIndex, bool isOnBeat)
        {
            OnFinisher(playerTransform, state, isOnBeat);
        }

        public override void OnFinisher(Transform playerTransform, AttackState state, bool isOnBeat)
        {
            if (playerTransform.TryGetComponent<MonoBehaviour>(out var runner))
            {
                runner.StartCoroutine(InfinityBarrageRoutine(playerTransform, isOnBeat));
            }
        }

        private IEnumerator InfinityBarrageRoutine(Transform playerTransform, bool isOnBeat)
        {
            if (finisherDelay > 0f) yield return new WaitForSeconds(finisherDelay);
            if (playerTransform == null || ObjectPooler.Instance == null) yield break;

            GameObject prefabToSpawn = finisherProjectilePrefab != null ? finisherProjectilePrefab : projectilePrefab;
            if (prefabToSpawn == null) yield break;

            float angleStep = 360f / Mathf.Max(1, bulletCount);
            float dmg = isOnBeat ? finisherDamage : damage * mistimedDamageMultiplier;

            for (int i = 0; i < bulletCount; i++)
            {
                float angle = i * angleStep;
                Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.right;
                Vector3 spawnPos = playerTransform.position + (dir * spawnRadiusOffset);

                GameObject projObj = ObjectPooler.Instance.Get(prefabToSpawn, spawnPos, Quaternion.Euler(0, 0, angle));
                
                if (projObj.TryGetComponent<Projectile>(out var projectile))
                {
                    projectile.InitializeProjectile(finisherSpeed, dmg, finisherStunLength, finisherKnockbackStrength, lifeTime, finisherProjectilePierce);
                    projectile.Fire(dir);
                }

                if (timeBetweenShots > 0f)
                {
                    yield return new WaitForSeconds(timeBetweenShots);
                }
            }
        }
    }
}