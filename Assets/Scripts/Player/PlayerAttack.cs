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
        public float comboWindow = 1.0f;
        public float attackCancelTime = 0.5f;

        [Header("State (Read Only)")]
        [SerializeField] AttackState _currentState = AttackState.Idle;

        Animator _animator;
        PlayerMovement _playerMovement;
        AudioHandler _audioHandler;
        RhythmInputHandler _rhythmInputHandler;

        int _currentAttackIndex = 0;
        float _timeSinceLastAttack = 0f;
        bool _comboFinished = false;

        void Awake()
        {
            _animator = GetComponent<Animator>();
            _playerMovement = GetComponent<PlayerMovement>();
            
            if (_playerMovement != null)
            {
                _playerMovement.OnDashStart += CancelAttack;
            }

            _audioHandler = FindAnyObjectByType<AudioHandler>();
            _rhythmInputHandler = FindAnyObjectByType<RhythmInputHandler>();
        }

        void Start()
        {
            if (_audioHandler != null)
            {
                comboWindow = _audioHandler.GetSecondsPerBeat();
            }
        }

        void OnDestroy()
        {
            if (_playerMovement != null)
            {
                _playerMovement.OnDashStart -= CancelAttack;
            }
        }

        private void Update()
        {
            _timeSinceLastAttack += Time.deltaTime;
            UpdateStateTransitions();
        }

        private void UpdateStateTransitions()
        {
            if (_currentState == AttackState.Idle || _currentState == AttackState.Special) return;

            if (_timeSinceLastAttack > attackCancelTime + comboWindow)
            {
                ReturnToIdle();
            }
        }

        #region Input Triggers

        public void OnMeleeAttack(InputValue value)
        {
            if (value.isPressed && CanAttack())
            {
                bool isOnBeat = false;
                if (_rhythmInputHandler != null)
                {
                    isOnBeat = _rhythmInputHandler.ProcessInput();
                    if (!isOnBeat) ResetCombo();
                }

                ExecuteAttack(isRanged: false, isOnBeat);
            } 
        }

        public void OnRangedAttack(InputValue value)
        {
            if (value.isPressed && CanAttack())
            {
                bool isOnBeat = false;
                if (_rhythmInputHandler != null)
                {
                    isOnBeat = _rhythmInputHandler.ProcessInput();
                    if (!isOnBeat) ResetCombo();
                }

                ExecuteAttack(isRanged: true, isOnBeat);
            }
        }

        #endregion

        bool CanAttack() 
        {
            if (_currentState == AttackState.Idle) return true;
            return _timeSinceLastAttack >= attackCancelTime;
        }

        private void ExecuteAttack(bool isRanged, bool isOnBeat)
        {
            if (_currentState == AttackState.Special) return;

            // Cast to your specific weapon classes to access the animation strings you moved
            MeleeWeapon meleeWeapon = equippedMeleeWeapon as MeleeWeapon;
            RangedWeapon rangedWeapon = equippedRangedWeapon as RangedWeapon;

            // The melee weapon still dictates the universal max combo length
            int maxComboLength = 0;
            if (meleeWeapon != null && meleeWeapon.comboAnimations != null)
            {
                maxComboLength = meleeWeapon.comboAnimations.Length;
            }

            if (maxComboLength == 0) return;

            if (_comboFinished)
            {
                ResetCombo();
            }

            string animToPlay = "";
            if (isRanged)
            {
                if (rangedWeapon != null)
                {
                    if (_currentAttackIndex == maxComboLength - 1) animToPlay = rangedWeapon.finisherAnimation;
                    else animToPlay = rangedWeapon.fireAnimation;
                }
            }
            else 
            {
                if (meleeWeapon != null)
                {
                    animToPlay = meleeWeapon.comboAnimations[_currentAttackIndex];
                }
            }

            // Evaluate weapon logic right before incrementing the index so the weapon receives the 0-based hit index
            if (isRanged && equippedRangedWeapon != null)
            {
                if (_currentAttackIndex == maxComboLength - 1) 
                    equippedRangedWeapon.OnFinisher(transform, _currentState, isOnBeat);
                else 
                    equippedRangedWeapon.OnAttack(transform, _currentState, _currentAttackIndex, isOnBeat);
            }
            else if (!isRanged && equippedMeleeWeapon != null)
            {
                if (_currentAttackIndex == maxComboLength - 1) 
                    equippedMeleeWeapon.OnFinisher(transform, _currentState, isOnBeat);
                else 
                    equippedMeleeWeapon.OnAttack(transform, _currentState, _currentAttackIndex, isOnBeat);
            }

            _currentAttackIndex++;

            if (_currentAttackIndex >= maxComboLength) _comboFinished = true;
            else _comboFinished = false;

            _currentState = isRanged ? AttackState.Ranged : AttackState.Melee;
            _timeSinceLastAttack = 0f; 

            if (_playerMovement != null)
            {
                _playerMovement.EnterAttackState();
            }

            if (_animator != null && !string.IsNullOrEmpty(animToPlay))
            {
                _animator.Play(animToPlay, 0, 0f);
            }
        }

        public void ExecuteSpecialSleight(string sleightAnimation)
        {
            _currentState = AttackState.Special;
            _timeSinceLastAttack = 0f;

            if (_playerMovement != null) _playerMovement.EnterAttackState();

            if (_animator != null)
            {
                _animator.Play(sleightAnimation, 0, 0f);
            }

            ResetCombo();
        }

        public void ResetCombo()
        {
            if (_currentAttackIndex != 0 || _comboFinished)
            {
                _currentAttackIndex = 0;
                _comboFinished = false;
            }
        }

        public void CancelAttack()
        {
            if (_currentState == AttackState.Idle || _currentState == AttackState.Special) return;

            _currentState = AttackState.Idle;
            _animator.Play("Idle");
            _comboFinished = true;
        }

        private void ReturnToIdle()
        {
            _currentState = AttackState.Idle;
            _timeSinceLastAttack = 0f;
            ResetCombo();
            
            if (_playerMovement != null)
            {
                _playerMovement.FreeAttackState();
            }
        }
    }
}