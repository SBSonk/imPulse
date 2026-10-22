using UnityEngine;
using ImPulse.Player;

namespace ImPulse.Weapons
{
    public abstract class RangedWeapon : NormalWeapon
    {
        [Header("Timing")]
        public float fireDelay = 0.1f;
        public float finisherDelay = 0.2f;

        [Header("Ranged Properties")]
        public GameObject projectilePrefab;
        public float projectileSpeed = 20f;
        public int projectilePierce = 1;
        public float lifeTime = 5f; 
 
        [Header("Finisher Properties")]
        public GameObject finisherProjectilePrefab; 
        public float finisherSpeed = 22f;
        public int finisherProjectilePierce = 1;

        [Header("Animations")]
        public string fireAnimation;
        public string finisherAnimation;
    }
}