using UnityEngine;
using ImPulse.Player;

namespace ImPulse.Weapons
{
    public abstract class RangedWeapon : Weapon
    {
        [Header("Ranged Properties")]
        public GameObject projectilePrefab;
        public float projectileSpeed = 20f;
        public float maxRange = 15f;

        [Header("Animations")]
        public string fireAnimation, finisherAnimation;
        
        // Common Ranged helper functions can go here
        // like projectile spawning/pooling logic.
    }
}