using UnityEngine;
using ImPulse.Player;

namespace ImPulse.Weapons
{
    public abstract class MeleeWeapon : Weapon
    {
        [Header("Melee Properties")]
        public float hitRadius = 1.5f;
        
        [Header("Animations")]
        public string[] comboAnimations;
        
        public LayerMask enemyLayer;
        
        // You can add common Melee helper functions here
        // like a protected OverlapSphere function to handle damage calculations.
    }
}