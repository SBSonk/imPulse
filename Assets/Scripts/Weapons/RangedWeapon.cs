using UnityEngine;
using ImPulse.Player;

namespace ImPulse.Weapons
{
    public abstract class RangedWeapon : Weapon
    {
        [Header("Timing")]
        public float fireDelay = 0.1f;
        public float finisherDelay = 0.2f;

        [Header("Ranged Properties")]
        public GameObject projectilePrefab;
        public float projectileDamage = 10f;
        public float projectileSpeed = 20f;
        public float maxRange = 15f;
        public float lifeTime = 5f;

        [Header("Finisher Properties")]
        public GameObject finisherProjectilePrefab; 
        public float finisherDamage = 15f;
        public float finisherSpeed = 22f;
        [Tooltip("The angle in degrees between the center, left, and right projectiles.")]
        public float finisherSpreadAngle = 20f; 

        [Header("Animations")]
        public string fireAnimation;
        public string finisherAnimation;
    }
}