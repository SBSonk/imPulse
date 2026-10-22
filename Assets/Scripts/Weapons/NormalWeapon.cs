using UnityEngine;
using ImPulse.Player;

namespace ImPulse.Weapons
{
    public abstract class NormalWeapon : Weapon
    {
        [Header("Mistimed Penalties")]
        public float mistimedDamageMultiplier = 0.5f;

        [Header("Finisher Stats")]
        public float finisherDamage = 20f;
        public float finisherStunLength = 1f;
        public float finisherKnockbackStrength = 10f;
    }
}