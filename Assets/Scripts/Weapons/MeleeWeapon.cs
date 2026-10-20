using UnityEngine;
using ImPulse.Player;
using ImPulse.Core;

namespace ImPulse.Weapons
{
    public abstract class MeleeWeapon : Weapon
    {
        [Header("Melee Properties")]
        public float hitRadius = 1.5f;
        
        [Header("Animations")]
        public string[] comboAnimations;
        
        public CollisionLayerMask enemyLayer;
        
        // You can add common Melee helper functions here
        // like a protected OverlapSphere function to handle damage calculations.
    }
}