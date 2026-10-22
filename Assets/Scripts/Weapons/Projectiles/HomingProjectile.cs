using UnityEngine;
using System.Collections.Generic;
using ImPulse.Core;

namespace ImPulse.Weapons
{
    public class HomingProjectile : BasicProjectile
    {
        [Header("Homing Settings")]
        public float homingTurnSpeed = 360f;
        public float homingDelay = 0.2f;
        
        [Header("Homing Timeout")]
        public bool stopHomingAfterTime = false;
        public float homingDuration = 1.0f;
        
        [Header("Targeting")]
        public CollisionLayerMask targetLayer = CollisionLayerMask.Enemy;

        private float _aliveTime = 0f;

        public override void OnPoolGet(GameObject sourcePrefab)
        {
            base.OnPoolGet(sourcePrefab);
            _aliveTime = 0f;
        }

        public override void OnPoolReturn()
        {
            base.OnPoolReturn();
            _aliveTime = 0f;
        }

        protected override void Update()
        {
            _aliveTime += Time.deltaTime;
            base.Update();
        }

        protected override void HandleMovement()
        {
            bool isHomingActive = _aliveTime >= homingDelay;

            // Disable homing if the optional timer is active and the duration has passed
            if (stopHomingAfterTime && _aliveTime >= (homingDelay + homingDuration))
            {
                isHomingActive = false;
            }

            if (isHomingActive)
            {
                Transform target = FindNearestTarget(transform.position);
                if (target != null)
                {
                    Vector3 targetDir = (target.position - transform.position).normalized;
                    
                    float currentAngle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
                    float targetAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;

                    float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, homingTurnSpeed * Time.deltaTime);
                    float rad = newAngle * Mathf.Deg2Rad;
                    
                    _direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f).normalized;
                    transform.rotation = Quaternion.Euler(0, 0, newAngle);
                }
            }

            base.HandleMovement();
        }

        private Transform FindNearestTarget(Vector3 position)
        {
            List<CustomCollider> enemies = CustomCollider.GetColliders(targetLayer);
            Transform bestTarget = null;
            float closestDistSqr = float.MaxValue;

            foreach (var col in enemies)
            {
                if (col == null || !col.gameObject.activeInHierarchy || !col.enabled) continue;
                
                if (_hitTargets.Contains(col)) continue;

                float distSqr = (col.transform.position - position).sqrMagnitude;
                if (distSqr < closestDistSqr)
                {
                    closestDistSqr = distSqr;
                    bestTarget = col.transform;
                }
            }

            return bestTarget;
        }
    }
}