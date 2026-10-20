using System;
using System.Collections.Generic;
using UnityEngine;
using ImPulse.Helpers;
using UnityEngine.AI;
using Unity.VisualScripting;

namespace ImPulse.Core
{
    public enum CollisionLayer
    {
        Player,
        Enemy,
        Projectile,
        Environment
    }

    [Flags]
    public enum CollisionLayerMask
    {
        None = 0,
        Player = 1 << 0,
        Enemy = 1 << 1,
        Projectile = 1 << 2,
        Environment = 1 << 3,
        All = ~0
    }

    public enum ColliderType
    {
        Circle,
        Square
    }

    public class CustomCollider : MonoBehaviour
    {
        public static List<CustomCollider>[] LayerGroups = null;

        [Header("Collision Settings")]
        public CollisionLayer layer = CollisionLayer.Enemy;
        public ColliderType colliderType = ColliderType.Circle;
        public float radius = 0.5f;
        public CollisionLayerMask layersToCheck;

        public event Action<CustomCollider> CollisionEnter;
        public event Action<CustomCollider> CollisionStay;
        public event Action<CustomCollider> CollisionExit;

        HashSet<CustomCollider> _activeCollisions = new HashSet<CustomCollider>();
        List<CustomCollider> _exitBuffer = new List<CustomCollider>();

        void Awake()
        {
            if (LayerGroups == null)
            {
                // initialize layer lookups
                int layerCount = Enum.GetValues(typeof(CollisionLayer)).Length;
                LayerGroups = new List<CustomCollider>[layerCount];
                for (int i = 0; i < layerCount; i++) LayerGroups[i] = new List<CustomCollider>();
            }
        }

        void OnEnable()
        {
            LayerGroups[(int)layer].Add(this);
        }

        void OnDisable()
        {
            LayerGroups[(int)layer].Remove(this);
            _activeCollisions.Clear();
        }

        void OnDestroy()
        {
            LayerGroups[(int)layer].Remove(this);
            _activeCollisions.Clear();
        }

        protected virtual void Update()
        {
            if (layersToCheck == CollisionLayerMask.None) return;

            // collision pass
            for (int i = 0; i < LayerGroups.Length; i++)
            {
                if (((int)layersToCheck & (1 << i)) != 0)
                {
                    var group = LayerGroups[i];
                    for (int j = group.Count - 1; j >= 0; j--)
                    {
                        CustomCollider other = group[j];
                        if (other == this || other == null || !other.isActiveAndEnabled) continue;

                        if (CheckOverlap(this, other))
                        {
                            if (_activeCollisions.Add(other))
                            {
                                CollisionEnter?.Invoke(other);
                            }
                            CollisionStay?.Invoke(other);
                        }
                    }
                }
            }

            // pass to remove collisions that ended
            if (_activeCollisions.Count > 0)
            {
                _exitBuffer.Clear();
                foreach (var other in _activeCollisions)
                {
                    if (other == null || !other.isActiveAndEnabled || !CheckOverlap(this, other))
                    {
                        _exitBuffer.Add(other);
                    }
                }

                for (int i = 0; i < _exitBuffer.Count; i++)
                {
                    _activeCollisions.Remove(_exitBuffer[i]);
                    if (_exitBuffer[i] != null)
                    {
                        CollisionExit?.Invoke(_exitBuffer[i]);
                    }
                }
            }
        }

        public static bool CheckOverlap(CustomCollider a, CustomCollider b)
        {
            if (a.colliderType == ColliderType.Circle && b.colliderType == ColliderType.Circle)
                return CollisionHelper.CircleToCircle(a.transform.position, a.radius, b.transform.position, b.radius);

            if (a.colliderType == ColliderType.Square && b.colliderType == ColliderType.Square)
                return CollisionHelper.BoxToBox(a.transform.position, a.radius, b.transform.position, b.radius);

            if (a.colliderType == ColliderType.Square && b.colliderType == ColliderType.Circle)
                return CollisionHelper.BoxToCircle(a.transform.position, a.radius, b.transform.position, b.radius);

            return CollisionHelper.BoxToCircle(b.transform.position, b.radius, a.transform.position, a.radius);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            if (colliderType == ColliderType.Circle)
            {
                Gizmos.DrawWireSphere(transform.position, radius);
            }
            else
            {
                Gizmos.DrawWireCube(transform.position, Vector3.one * radius * 2f); 
            }
        }
    }
}