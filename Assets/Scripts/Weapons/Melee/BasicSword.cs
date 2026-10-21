using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ImPulse.Player;
using ImPulse.Core;
using ImPulse.Helpers;
using ImPulse.Enemies;

namespace ImPulse.Weapons
{
    [CreateAssetMenu(fileName = "New Basic Sword", menuName = "ImPulse/Weapons/Basic Sword")]
    public class BasicSword : MeleeWeapon
    {
        public override void OnAttack(Transform playerTransform, AttackState state, int comboIndex, bool isOnBeat)
        {
            MonoBehaviour runner = playerTransform.GetComponent<MonoBehaviour>();
            if (runner != null)
            {
                runner.StartCoroutine(AttackRoutine(playerTransform, comboIndex, isOnBeat));
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

        private IEnumerator AttackRoutine(Transform playerTransform, int comboIndex, bool isOnBeat)
        {
            if (attackDelay > 0f) yield return new WaitForSeconds(attackDelay);
            if (playerTransform == null) yield break;

            float calculatedDamage = damage;
            
            Vector3 forwardDir = playerTransform.right; 
            Vector3 hitboxCenter = playerTransform.position + (forwardDir * forwardOffset);

            ProcessMeleeHitbox(playerTransform, hitboxCenter, hitRadius, calculatedDamage);
        }

        private IEnumerator FinisherRoutine(Transform playerTransform, bool isOnBeat)
        {
            if (finisherDelay > 0f) yield return new WaitForSeconds(finisherDelay);
            if (playerTransform == null) yield break;

            float finisherDamage = damage * 2f;
            
            Vector3 forwardDir = playerTransform.right; 
            Vector3 hitboxCenter = playerTransform.position + (forwardDir * forwardOffset);

            ProcessMeleeHitbox(playerTransform, hitboxCenter, hitRadius * 1.5f, finisherDamage);
        }

        private void ProcessMeleeHitbox(Transform attackerTransform, Vector3 center, float radius, float damage)
        {
            List<CustomCollider> targets = CustomCollider.GetColliders(enemyLayer);

            foreach (CustomCollider targetCollider in targets)
            {
                if (targetCollider.gameObject == attackerTransform.gameObject) continue;

                HasHealth entity = targetCollider.GetComponent<HasHealth>();
                if (entity == null) continue;

                float targetRadius = targetCollider.radius;
                bool isHit = false;

                // Route the check based on the target's collider type
                if (targetCollider.colliderType == ColliderType.Circle)
                {
                    isHit = CollisionHelper.CircleToCircle(
                        center, 
                        radius, 
                        targetCollider.transform.position, 
                        targetRadius
                    );
                }
                else if (targetCollider.colliderType == ColliderType.Square)
                {
                    // Remember: BoxToCircle parameters are (boxPos, boxRadius, circlePos, circleRadius)
                    isHit = CollisionHelper.BoxToCircle(
                        targetCollider.transform.position, 
                        targetRadius,
                        center, 
                        radius
                    );
                }

                if (isHit)
                {
                    entity.TakeDamage(damage);

                    Vector3 knockbackDir = entity.transform.position - attackerTransform.position;
                    knockbackDir.Normalize();

                    if (entity is PlayerStatus)
                    {
                        if (entity.TryGetComponent<PlayerMovement>(out var movement)) movement.ApplyKnockback(knockbackDir, knockbackStrength, stunLength);
                    }

                    if (entity is Enemy)
                    {
                        var enemy = entity as Enemy;
                        enemy.ApplyKnockback(knockbackDir, knockbackStrength, stunLength);
                    }
                    
                    // TODO: Instantiate hit VFX via ObjectPooler.Instance here
                }
            }
        }
    }
}