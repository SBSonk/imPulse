using UnityEngine;
using ImPulse.Player;

namespace ImPulse.Weapons
{
    public abstract class SleightWeapon : Weapon
    {
        [Header("Sleight Properties")]
        public float cooldown = 5f;
        public string animationName = "SleightAnim";
        public float sleightDelay = 0.2f;

        public override void OnAttack(Transform playerTransform, AttackState state, int comboIndex, bool isOnBeat)
        {
            ExecuteSleight(playerTransform, state);
        }

        public override void OnFinisher(Transform playerTransform, AttackState state, bool isOnBeat)
        {
            ExecuteSleight(playerTransform, state);
        }

        public abstract void ExecuteSleight(Transform playerTransform, AttackState state);
    }
}