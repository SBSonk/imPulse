using UnityEngine;
using ImPulse.Player;

namespace ImPulse.Weapons
{
    [CreateAssetMenu(fileName = "New Basic Sword", menuName = "ImPulse/Weapons/Basic Sword")]
    public class BasicSword : MeleeWeapon
    {
        public override void OnAttack(Transform playerTransform, AttackState state, int comboIndex, bool isOnBeat)
        {
            // Example: Increase damage slightly on later combo hits
            float calculatedDamage = baseDamage + (comboIndex * 2f);
            
            UnityEngine.Debug.Log($"{weaponName} Swings! Hit #{comboIndex + 1}. Damage: {calculatedDamage}. State: {state}");
            
            // TODO: Implement actual hit detection (e.g. Physics.OverlapSphere)
            // using hitRadius and forwardOffset.
        }

        public override void OnFinisher(Transform playerTransform, AttackState state, bool isOnBeat)
        {
            UnityEngine.Debug.Log($"{weaponName} performs heavy MELEE FINISHER! Huge AoE! State: {state}");
            
            // TODO: Implement a larger, knockback-inducing hitbox
        }
    }
}