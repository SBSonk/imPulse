using System.Collections;
using UnityEngine;
using ImPulse.Player;
using ImPulse.Core.Pooling;

namespace ImPulse.Weapons
{
    [CreateAssetMenu(fileName = "Infinity Sleight Weapon", menuName = "ImPulse/Weapons/Sleights/Infinity")]
    public class InfinitySleightWeapon : SleightWeapon
    {
        [Header("Projectile Properties")]
        public GameObject projectilePrefab;
        public float projectileSpeed = 22f;
        public int projectilePierce = 1;
        public float lifeTime = 5f;

        [Header("Infinity Settings")]
        public int bulletCount = 12;
        public float spawnRadiusOffset = 0.75f;
        public float timeBetweenShots = 0.05f;

        public override void ExecuteSleight(Transform playerTransform, AttackState state)
        {
            if (playerTransform.TryGetComponent<MonoBehaviour>(out var runner))
            {
                runner.StartCoroutine(InfinityBarrageRoutine(playerTransform));
            }
        }

        private IEnumerator InfinityBarrageRoutine(Transform playerTransform)
        {
            if (sleightDelay > 0f) yield return new WaitForSeconds(sleightDelay);
            if (playerTransform == null || ObjectPooler.Instance == null || projectilePrefab == null) yield break;

            float angleStep = 360f / Mathf.Max(1, bulletCount);

            for (int i = 0; i < bulletCount; i++)
            {
                float angle = i * angleStep;
                Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.right;
                Vector3 spawnPos = playerTransform.position + (dir * spawnRadiusOffset);

                GameObject projObj = ObjectPooler.Instance.Get(projectilePrefab, spawnPos, Quaternion.Euler(0, 0, angle));
                
                if (projObj.TryGetComponent<Projectile>(out var projectile))
                {
                    projectile.InitializeProjectile(projectileSpeed, damage, stunLength, knockbackStrength, lifeTime, projectilePierce);
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