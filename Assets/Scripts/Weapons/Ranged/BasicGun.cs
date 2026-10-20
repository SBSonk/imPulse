using System.Collections;
using UnityEngine;
using ImPulse.Player;
using ImPulse.Core;
using ImPulse.Core.Pooling;

namespace ImPulse.Weapons
{
    [CreateAssetMenu(fileName = "New Basic Gun", menuName = "ImPulse/Weapons/Basic Gun")]
    public class BasicGun : RangedWeapon
    {
        public override void OnAttack(Transform playerTransform, AttackState state, int comboIndex, bool isOnBeat)
        {
            PlayerStatus player = playerTransform.GetComponent<PlayerStatus>();
            if (player != null)
            {
                player.StartCoroutine(FireRoutine(playerTransform));
            }
        }

        public override void OnFinisher(Transform playerTransform, AttackState state, bool isOnBeat)
        {
            PlayerStatus player = playerTransform.GetComponent<PlayerStatus>();
            if (player != null)
            {
                player.StartCoroutine(FinisherRoutine(playerTransform));
            }
        }

        private IEnumerator FireRoutine(Transform playerTransform)
        {
            if (fireDelay > 0f)
            {
                yield return new WaitForSeconds(fireDelay);
            }

            if (playerTransform == null || projectilePrefab == null || ObjectPooler.Instance == null) yield break;

            Vector3 fireDirection = playerTransform.right; 
            float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;
            
            GameObject projObj = ObjectPooler.Instance.Get(projectilePrefab, playerTransform.GetComponent<PlayerAttack>().GetBulletOrigin().position, Quaternion.Euler(0, 0, angle));
            Projectile projectile = projObj.GetComponent<Projectile>();
            
            if (projectile != null)
            {
                projectile.InitializeProjectile(projectileSpeed, projectileDamage, lifeTime);
                projectile.Fire(fireDirection);
            }
        }

        private IEnumerator FinisherRoutine(Transform playerTransform)
        {
            if (finisherDelay > 0f)
            {
                yield return new WaitForSeconds(finisherDelay);
            }

            if (playerTransform == null || ObjectPooler.Instance == null) yield break;

            GameObject prefabToSpawn = finisherProjectilePrefab != null ? finisherProjectilePrefab : projectilePrefab;

            if (prefabToSpawn != null)
            {
                Vector3 originPos = playerTransform.GetComponent<PlayerAttack>().GetBulletOrigin().position;
                Vector3 centerDir = playerTransform.right;

                SpawnProjectile(prefabToSpawn, originPos, centerDir, finisherSpeed, finisherDamage);

                Vector3 leftDir = Quaternion.Euler(0, 0, finisherSpreadAngle) * centerDir;
                SpawnProjectile(prefabToSpawn, originPos, leftDir, finisherSpeed, finisherDamage);

                Vector3 rightDir = Quaternion.Euler(0, 0, -finisherSpreadAngle) * centerDir;
                SpawnProjectile(prefabToSpawn, originPos, rightDir, finisherSpeed, finisherDamage);
            }
        }

        private void SpawnProjectile(GameObject prefab, Vector3 startPos, Vector3 direction, float speed, float damage)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            GameObject projObj = ObjectPooler.Instance.Get(prefab, startPos, Quaternion.Euler(0, 0, angle));
            Projectile projectile = projObj.GetComponent<Projectile>();
            
            if (projectile != null)
            {
                projectile.InitializeProjectile(speed, damage, lifeTime);
                projectile.Fire(direction);
            }
        }
    }
}