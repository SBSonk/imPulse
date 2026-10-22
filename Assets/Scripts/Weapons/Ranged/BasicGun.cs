using System.Collections;
using UnityEngine;
using ImPulse.Player;
using ImPulse.Core;
using ImPulse.Core.Pooling;
using ImPulse.Enemies;

namespace ImPulse.Weapons
{
    [CreateAssetMenu(fileName = "New Basic Gun", menuName = "ImPulse/Weapons/Basic Gun")]
    public class BasicGun : RangedWeapon
    {
        [Header("Normal Shot Spread")]
        public int bulletCount = 1;
        public float spreadAngle = 15f;

        [Header("Finisher Shot Spread")]
        public int finisherBulletCount = 3;
        public float finisherSpreadAngle = 20f;

        public override void OnAttack(Transform playerTransform, AttackState state, int comboIndex, bool isOnBeat)
        {
            if (playerTransform.TryGetComponent<MonoBehaviour>(out var runner))
            {
                runner.StartCoroutine(FireRoutine(playerTransform, isOnBeat));
            }
        }

        public override void OnFinisher(Transform playerTransform, AttackState state, bool isOnBeat)
        {
            if (playerTransform.TryGetComponent<MonoBehaviour>(out var runner))
            {
                runner.StartCoroutine(FinisherRoutine(playerTransform, isOnBeat));
            }
        }

        private Vector3 GetOriginPos(Transform root)
        {
            if (root.TryGetComponent<PlayerAttack>(out var pAttack) && pAttack.GetBulletOrigin() != null) 
                return pAttack.GetBulletOrigin().position;

            if (root.TryGetComponent<BasicShooterEnemy>(out var enemy)) 
                return enemy.GetBulletOrigin().position;

            return root.position;
        }

        private IEnumerator FireRoutine(Transform playerTransform, bool isOnBeat)
        {
            if (fireDelay > 0f) yield return new WaitForSeconds(fireDelay);
            if (playerTransform == null || projectilePrefab == null || ObjectPooler.Instance == null) yield break;

            Vector3 centerDir = playerTransform.right;
            Vector3 originPos = GetOriginPos(playerTransform);
            
            float d = isOnBeat ? damage : damage * mistimedDamageMultiplier;
            
            FireSpread(projectilePrefab, originPos, centerDir, projectileSpeed, d, stunLength, knockbackStrength, projectilePierce, bulletCount, spreadAngle);
        }

        private IEnumerator FinisherRoutine(Transform playerTransform, bool isOnBeat)
        {
            if (finisherDelay > 0f) yield return new WaitForSeconds(finisherDelay);
            if (playerTransform == null || ObjectPooler.Instance == null) yield break;

            GameObject prefabToSpawn = finisherProjectilePrefab != null ? finisherProjectilePrefab : projectilePrefab;

            if (prefabToSpawn != null)
            {
                Vector3 originPos = GetOriginPos(playerTransform);
                Vector3 centerDir = playerTransform.right;

                float d = isOnBeat ? finisherDamage : damage * mistimedDamageMultiplier;

                FireSpread(prefabToSpawn, originPos, centerDir, finisherSpeed, d, finisherStunLength, finisherKnockbackStrength, finisherProjectilePierce, finisherBulletCount, finisherSpreadAngle);
            }
        }

        private void FireSpread(GameObject prefab, Vector3 startPos, Vector3 centerDir, float speed, float dmg, float stun, float knockback, int pierces, int count, float maxSpread)
        {
            if (count <= 1)
            {
                SpawnProjectile(prefab, startPos, centerDir, speed, dmg, stun, knockback, pierces);
                return;
            }

            float startAngle = -maxSpread / 2f;
            float angleStep = maxSpread / (count - 1);

            for (int i = 0; i < count; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                Vector3 fireDir = Quaternion.Euler(0, 0, currentAngle) * centerDir;
                SpawnProjectile(prefab, startPos, fireDir, speed, dmg, stun, knockback, pierces);
            }
        }

        private void SpawnProjectile(GameObject prefab, Vector3 startPos, Vector3 direction, float speed, float dmg, float stun, float knockback, int pierces = 1)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            GameObject projObj = ObjectPooler.Instance.Get(prefab, startPos, Quaternion.Euler(0, 0, angle));
            
            if (projObj.TryGetComponent<Projectile>(out var projectile))
            {
                projectile.InitializeProjectile(speed, dmg, stun, knockback, lifeTime, pierces);
                projectile.Fire(direction);
            }
        }
    }
}