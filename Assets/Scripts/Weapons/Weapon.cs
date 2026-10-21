using UnityEngine;
using ImPulse.Player; // To access the AttackState enum

namespace ImPulse.Weapons
{
    public abstract class Weapon : ScriptableObject
    {
        [Header("Core Weapon Stats")]
        public string weaponName = "New Weapon";

        [Header("Damage Stats")]
        public float damage = 10;
        public float finisherDamage = 15f;
        public float stunLength = .25f;
        public float knockbackStrength = 5;
        public float mistimedDamageMultiplier = 0.25f;
        

        /// <summary>
        /// Called on every standard hit of the combo.
        /// </summary>
        /// <param name="playerTransform">The player's transform for spawning hitboxes/projectiles.</param>
        /// <param name="state">The current AttackState (Melee, Ranged, etc).</param>
        /// <param name="comboIndex">Which hit in the combo this is (e.g. 0, 1, 2).</param>
        public abstract void OnAttack(Transform playerTransform, AttackState state, int comboIndex, bool isOnBeat);

        /// <summary>
        /// Called ONLY on the final hit of a combo sequence.
        /// </summary>
        /// <param name="playerTransform">The player's transform.</param>
        /// <param name="state">The current AttackState.</param>
        public abstract void OnFinisher(Transform playerTransform, AttackState state, bool isOnBeat);
    }
}