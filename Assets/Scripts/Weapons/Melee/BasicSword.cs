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
                float d = isOnBeat ? damage : damage*mistimedDamageMultiplier;
                runner.StartCoroutine(AttackRoutine(playerTransform, comboIndex, isOnBeat, d));
            }
        }

        public override void OnFinisher(Transform playerTransform, AttackState state, bool isOnBeat)
        {
            MonoBehaviour runner = playerTransform.GetComponent<MonoBehaviour>();
            if (runner != null)
            {
                float d = isOnBeat ? finisherDamage : damage*mistimedDamageMultiplier;
                runner.StartCoroutine(FinisherRoutine(playerTransform, isOnBeat, d));
            }
        }

        private IEnumerator AttackRoutine(Transform playerTransform, int comboIndex, bool isOnBeat, float calculatedDamage)
        {
            if (attackDelay > 0f) yield return new WaitForSeconds(attackDelay);
            if (playerTransform == null) yield break;

            Vector3 forwardDir = playerTransform.right;
            Vector3 hitboxCenter = playerTransform.position + (forwardDir * forwardOffset);

            // Passes standard stun and knockback
            ProcessMeleeHitbox(playerTransform, hitboxCenter, hitRadius, calculatedDamage, knockbackStrength, stunLength);
        }

        private IEnumerator FinisherRoutine(Transform playerTransform, bool isOnBeat, float finisherDmg)
        {
            if (finisherDelay > 0f) yield return new WaitForSeconds(finisherDelay);
            if (playerTransform == null) yield break;

            Vector3 forwardDir = playerTransform.right;
            Vector3 hitboxCenter = playerTransform.position + (forwardDir * forwardOffset);

            // Passes finisher stun and knockback
            ProcessMeleeHitbox(playerTransform, hitboxCenter, hitRadius * 1.5f, finisherDmg, finisherKnockbackStrength, finisherStunLength);
        }

        // Updated signature to take specific stun and knockback values
        private void ProcessMeleeHitbox(Transform attackerTransform, Vector3 center, float radius, float dmg, float knockback, float stun)
        {
            List<CustomCollider> targets = CustomCollider.GetColliders(enemyLayer);

            foreach (CustomCollider targetCollider in targets)
            {
                if (targetCollider.gameObject == attackerTransform.gameObject) continue;

                HasHealth entity = targetCollider.GetComponent<HasHealth>();
                if (entity == null) continue;

                float targetRadius = targetCollider.radius;
                bool isHit = false;

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
                    isHit = CollisionHelper.BoxToCircle(
                        targetCollider.transform.position, 
                        targetRadius,
                        center, 
                        radius
                    );
                }

                if (isHit)
                {
                    entity.TakeDamage(dmg);

                    Vector3 knockbackDir = entity.transform.position - attackerTransform.position;
                    knockbackDir.Normalize();

                    if (entity is PlayerStatus)
                    {
                        // Uses the locally passed knockback/stun parameters
                        if (entity.TryGetComponent<PlayerMovement>(out var movement)) movement.ApplyKnockback(knockbackDir, knockback, stun);
                    }

                    if (entity is Enemy)
                    {
                        var enemy = entity as Enemy;
                        // Uses the locally passed knockback/stun parameters
                        enemy.ApplyKnockback(knockbackDir, knockback, stun);
                    }

                    // TODO: Instantiate hit VFX via ObjectPooler.Instance here
                }
            }
        }
    }
}