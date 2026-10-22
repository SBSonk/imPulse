using UnityEngine;
using ImPulse.Player;
using ImPulse.Core;

namespace ImPulse.Weapons
{
    public abstract class MeleeWeapon : NormalWeapon
    {
        [Header("Timing")]
        public float attackDelay = 0.1f;
        public float finisherDelay = 0.2f;

        [Tooltip("The radius of the circular hitbox.")]
        public float hitRadius = 2f;

        [Tooltip("How far forward from the player's center the hitbox originates.")]
        public float forwardOffset = 1f;
        
        [Header("Animations")]
        public string[] comboAnimations;
        public string finisherAnimation;
        
        public CollisionLayerMask enemyLayer;
    }
}