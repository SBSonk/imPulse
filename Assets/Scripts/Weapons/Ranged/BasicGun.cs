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
        public override void OnAttack(Transform playerTransform, AttackState state, int comboIndex, bool isOnBeat)
        {
            MonoBehaviour runner = playerTransform.GetComponent<MonoBehaviour>();
            if (runner != null)
            {
                runner.StartCoroutine(FireRoutine(playerTransform, isOnBeat));
            }
        }

        public override void OnFinisher(Transform playerTransform, AttackState state, bool isOnBeat)
        {
            MonoBehaviour runner = playerTransform.GetComponent<MonoBehaviour>();
            if (runner != null)
            {
                runner.StartCoroutine(FinisherRoutine(playerTransform, isOnBeat));
            }
        }

        private Vector3 GetOriginPos(Transform root)
        {
            PlayerAttack pAttack = root.GetComponent<PlayerAttack>();
            if (pAttack != null && pAttack.GetBulletOrigin() != null) return pAttack.GetBulletOrigin().position;

            BasicShooterEnemy enemy = root.GetComponent<BasicShooterEnemy>();
            if (enemy != null) return enemy.GetBulletOrigin().position;

            return root.position;
        }

        private IEnumerator FireRoutine(Transform playerTransform, bool isOnBeat)
        {
            if (fireDelay > 0f) yield return new WaitForSeconds(fireDelay);
            if (playerTransform == null || projectilePrefab == null || ObjectPooler.Instance == null) yield break;

            Vector3 fireDirection = playerTransform.right;
            float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;
            Vector3 originPos = GetOriginPos(playerTransform);
            
            GameObject projObj = ObjectPooler.Instance.Get(projectilePrefab, originPos, Quaternion.Euler(0, 0, angle));
            Projectile projectile = projObj.GetComponent<Projectile>();
            
            if (projectile != null)
            {
                float d = isOnBeat ? damage : damage * mistimedDamageMultiplier;
                // Passes the standard stun and knockback values
                projectile.InitializeProjectile(projectileSpeed, d, stunLength, knockbackStrength, lifeTime);
                projectile.Fire(fireDirection);
            }
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

                // Passes the new finisher stun and knockback values
                SpawnProjectile(prefabToSpawn, originPos, centerDir, finisherSpeed, d, finisherStunLength, finisherKnockbackStrength);

                Vector3 leftDir = Quaternion.Euler(0, 0, finisherSpreadAngle) * centerDir;
                SpawnProjectile(prefabToSpawn, originPos, leftDir, finisherSpeed, d, finisherStunLength, finisherKnockbackStrength);

                Vector3 rightDir = Quaternion.Euler(0, 0, -finisherSpreadAngle) * centerDir;
                SpawnProjectile(prefabToSpawn, originPos, rightDir, finisherSpeed, d, finisherStunLength, finisherKnockbackStrength);
            }
        }

        // Updated signature to take specific stun and knockback values
        private void SpawnProjectile(GameObject prefab, Vector3 startPos, Vector3 direction, float speed, float dmg, float stun, float knockback)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            GameObject projObj = ObjectPooler.Instance.Get(prefab, startPos, Quaternion.Euler(0, 0, angle));
            Projectile projectile = projObj.GetComponent<Projectile>();
            
            if (projectile != null)
            {
                projectile.InitializeProjectile(speed, dmg, stun, knockback, lifeTime);
                projectile.Fire(direction);
            }
        }
    }
}