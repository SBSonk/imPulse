using ImPulse.Core;
using ImPulse.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ImPulse.Player
{
    public enum AttackState
    {
        Idle,
        Melee,
        Ranged,
        Special 
    }

    public class PlayerAttack : MonoBehaviour
    {
        [Header("Equipped Weapons")]
        public Weapon equippedMeleeWeapon;
        public Weapon equippedRangedWeapon;

        [Header("Timing Windows")]
        [Tooltip("How long before the player can input the next attack to continue the combo.")]
        public float attackCancelTime = 0.3f;
        [Tooltip("How long the attack state actually lasts before movement is naturally restored.")]
        public float attackDuration = 0.6f;

        [SerializeField] Transform bulletOrigin;

        [Header("State (Read Only)")]
        [SerializeField] AttackState _currentState = AttackState.Idle;

        Animator _animator;
        PlayerMovement _playerMovement;
        RhythmInputHandler _rhythmInputHandler;
        HasHealth _playerHealth;

        float _timeSinceLastAttack = 0f;
        float _lastHealthAmount;

        void Awake()
        {
            _animator = GetComponent<Animator>();
            _playerMovement = GetComponent<PlayerMovement>();
            
            if (_playerMovement != null)
            {
                _playerMovement.OnDashStart += CancelAttack;
            }

            _rhythmInputHandler = FindAnyObjectByType<RhythmInputHandler>();
        }

        void Start()
        {
            _playerHealth = GetComponent<HasHealth>();
            if (_playerHealth != null)
            {
                _lastHealthAmount = _playerHealth.currentHealth;
                _playerHealth.OnHealthChanged += HandleHealthChanged;
            }
        }

        void OnDestroy()
        {
            if (_playerMovement != null) _playerMovement.OnDashStart -= CancelAttack;
            if (_playerHealth != null) _playerHealth.OnHealthChanged -= HandleHealthChanged;
        }

        private void Update()
        {
            _timeSinceLastAttack += Time.deltaTime;
            UpdateStateTransitions();
        }

        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            if (currentHealth < _lastHealthAmount)
            {
                if (_rhythmInputHandler != null) _rhythmInputHandler.DropCombo();
            }
            _lastHealthAmount = currentHealth;
        }

        private void UpdateStateTransitions()
        {
            if (_currentState == AttackState.Idle || _currentState == AttackState.Special) return;

            // Only return to idle (and free movement) when the full attack animation duration has passed
            if (_timeSinceLastAttack > attackDuration)
            {
                ReturnToIdle(); 
            }
        }

        public void OnMeleeAttack(InputValue value)
        {
            if (value.isPressed && CanAttack())
            {
                bool isOnBeat = false;
                if (_rhythmInputHandler != null) isOnBeat = _rhythmInputHandler.ProcessInput();
                ExecuteAttack(isRanged: false, isOnBeat);
            } 
        }

        public void OnRangedAttack(InputValue value)
        {
            if (value.isPressed && CanAttack())
            {
                bool isOnBeat = false;
                if (_rhythmInputHandler != null) isOnBeat = _rhythmInputHandler.ProcessInput();
                ExecuteAttack(isRanged: true, isOnBeat);
            }
        }

        bool CanAttack() 
        {
            if (_currentState == AttackState.Idle) return true;
            
            // Allow the player to override the current attack early to continue the combo
            return _timeSinceLastAttack >= attackCancelTime;
        }

        private void ExecuteAttack(bool isRanged, bool isOnBeat)
        {
            if (_currentState == AttackState.Special || _rhythmInputHandler == null) return;

            bool isFinisher = _rhythmInputHandler.IsFinisher;
            int attackIndex = Mathf.Max(0, _rhythmInputHandler.CurrentCombo - 1);

            MeleeWeapon meleeWeapon = equippedMeleeWeapon as MeleeWeapon;
            RangedWeapon rangedWeapon = equippedRangedWeapon as RangedWeapon;
            string animToPlay = "";

            if (isRanged)
            {
                if (rangedWeapon != null)
                {
                    animToPlay = isFinisher ? rangedWeapon.finisherAnimation : rangedWeapon.fireAnimation;
                }
            }
            else 
            {
                if (meleeWeapon != null && meleeWeapon.comboAnimations != null && meleeWeapon.comboAnimations.Length > 0)
                {
                    animToPlay = isFinisher ? meleeWeapon.finisherAnimation : meleeWeapon.comboAnimations[attackIndex % meleeWeapon.comboAnimations.Length];
                }
            }

            if (isRanged && equippedRangedWeapon != null)
            {
                if (isFinisher) equippedRangedWeapon.OnFinisher(transform, _currentState, isOnBeat);
                else equippedRangedWeapon.OnAttack(transform, _currentState, attackIndex, isOnBeat);
            }
            else if (!isRanged && equippedMeleeWeapon != null)
            {
                if (isFinisher) equippedMeleeWeapon.OnFinisher(transform, _currentState, isOnBeat);
                else equippedMeleeWeapon.OnAttack(transform, _currentState, attackIndex, isOnBeat);
            }

            _currentState = isRanged ? AttackState.Ranged : AttackState.Melee;
            _timeSinceLastAttack = 0f; 

            if (_playerMovement != null) _playerMovement.EnterAttackState();
            if (_animator != null && !string.IsNullOrEmpty(animToPlay)) _animator.Play(animToPlay, 0, 0f);
        }

        public void ExecuteSpecialSleight(string sleightAnimation)
        {
            _currentState = AttackState.Special;
            _timeSinceLastAttack = 0f;

            if (_playerMovement != null) _playerMovement.EnterAttackState();
            if (_animator != null) _animator.Play(sleightAnimation, 0, 0f);

            if (_rhythmInputHandler != null) _rhythmInputHandler.DropCombo();
        }

        public void CancelAttack()
        {
            if (_currentState == AttackState.Idle || _currentState == AttackState.Special) return;
            _currentState = AttackState.Idle;
            _animator.Play("Idle");
            
            // Explicitly free movement if cancelled by a dash
            if (_playerMovement != null) _playerMovement.FreeAttackState();
        }

        private void ReturnToIdle()
        {
            _currentState = AttackState.Idle;
            _timeSinceLastAttack = 0f;
            if (_playerMovement != null) _playerMovement.FreeAttackState();
        }

        public Transform GetBulletOrigin() => bulletOrigin;
    }
}