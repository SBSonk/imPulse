using UnityEngine;
using ImPulse.Player;

namespace ImPulse.Weapons
{
    public abstract class Weapon : ScriptableObject
    {
        [Header("Core Weapon Stats")]
        public string weaponName = "New Weapon";

        [Header("Timing Stats (Fire Rate)")]
        [Tooltip("How long before the player can input the next attack to continue the combo.")]
        public float attackCancelTime = 0.3f;
        
        [Tooltip("How long the attack state actually lasts before movement is naturally restored.")]
        public float attackDuration = 0.6f;

        [Header("Standard Attack Stats")]
        public float damage = 10;
        public float stunLength = .25f;
        public float knockbackStrength = 5f;

        [Header("Audio")]
        public AudioClip attackSFX;
        [Range(0f, 1f)] public float soundVolume = 1f;

        public abstract void OnAttack(Transform playerTransform, AttackState state, int comboIndex, bool isOnBeat);
        public abstract void OnFinisher(Transform playerTransform, AttackState state, bool isOnBeat);
    }
}