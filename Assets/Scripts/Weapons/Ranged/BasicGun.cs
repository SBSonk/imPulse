using UnityEngine;
using ImPulse.Player;

namespace ImPulse.Weapons
{
    [CreateAssetMenu(fileName = "New Basic Gun", menuName = "ImPulse/Weapons/Basic Bow")]
    public class BasicGun : RangedWeapon
    {
        public override void OnAttack(Transform playerTransform, AttackState state, int comboIndex, bool isOnBeat)
        {
            UnityEngine.Debug.Log($"{weaponName} fires a normal arrow! State: {state}");
            
            // TODO: Instantiate projectilePrefab at playerTransform.position
            // and apply projectileSpeed.
        }

        public override void OnFinisher(Transform playerTransform, AttackState state, bool isOnBeat)
        {
            UnityEngine.Debug.Log($"{weaponName} fires a PIERCING FINISHER arrow! State: {state}");
            
            // TODO: Instantiate a different, larger projectile or fire 3 arrows in a spread.
        }
    }
}