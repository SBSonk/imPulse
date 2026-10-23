using ImPulse.Core;
using ImPulse.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace ImPulse.Player
{
    public enum AttackState
    {
        Idle,
        Melee,
        Ranged,
        Special 
    }

    public enum WeaponSlot
    {
        Melee,
        Ranged
    }

    public class PlayerAttack : MonoBehaviour
    {
        [Header("Equipped Weapons")]
        public Weapon equippedMeleeWeapon;
        public Weapon equippedRangedWeapon;
        
        [Header("Sleight State (Read Only)")]
        public List<SleightType> unlockedSleights = new List<SleightType>();
        public SleightType currentStashedSleight = SleightType.None;
        [SerializeField] private bool _isSleightOnCooldown = false;
        [SerializeField] private float _sleightCooldownTimer = 0f;

        [Header("Timing Windows")]
        public float attackCancelTime = 0.3f;
        public float attackDuration = 0.6f;

        [SerializeField] Transform bulletOrigin;

        [Header("State (Read Only)")]
        [SerializeField] AttackState _currentState = AttackState.Idle;

        AudioSource _sfxSource;

        Animator _animator;
        PlayerMovement _playerMovement;
        RhythmInputHandler _rhythmInputHandler;
        HasHealth _playerHealth;
        GameManager _gameManager;

        float _timeSinceLastAttack = 0f;
        float _lastHealthAmount;
        
        bool _isUpgrading = false;
        bool _isDashing = false;

        private Dictionary<SleightType, SleightWeapon> _sleightMap = new Dictionary<SleightType, SleightWeapon>();

        void Awake()
        {
            _animator = GetComponent<Animator>();
            _playerMovement = GetComponent<PlayerMovement>();
            
            if (_playerMovement != null)
            {
                _playerMovement.OnDashStart += CancelAttack;
                _playerMovement.OnDashStart += HandleDashStart;
                _playerMovement.OnDashEnd += HandleDashEnd; 
            }

            _rhythmInputHandler = FindAnyObjectByType<RhythmInputHandler>();
            if (_rhythmInputHandler != null)
            {
                _rhythmInputHandler.OnSleightTriggered += ExecuteSpecialSleight;
            }

            if (!TryGetComponent<AudioSource>(out var _)) gameObject.AddComponent<AudioSource>();
            _sfxSource = GetComponent<AudioSource>();

            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;
        }

        void Start()
        {
            _playerHealth = GetComponent<HasHealth>();
            if (_playerHealth != null)
            {
                _lastHealthAmount = _playerHealth.currentHealth;
                _playerHealth.OnHealthChanged += HandleHealthChanged;
            }

            _gameManager = FindAnyObjectByType<GameManager>();
            if (_gameManager != null)
            {
                _gameManager.OnLevelUp += HandleUpgradeStart;
                if (UpgradeManager.Instance != null)
                {
                    UpgradeManager.Instance.OnFinishedSelecting += HandleUpgradeFinish; 
                }
            }

            RollNextSleight();
        }

        void OnDestroy()
        {
            if (_playerMovement != null) 
            {
                _playerMovement.OnDashStart -= CancelAttack;
                _playerMovement.OnDashStart -= HandleDashStart;
                _playerMovement.OnDashEnd -= HandleDashEnd;
            }
            if (_rhythmInputHandler != null)
            {
                _rhythmInputHandler.OnSleightTriggered -= ExecuteSpecialSleight;
            }
            if (_playerHealth != null) _playerHealth.OnHealthChanged -= HandleHealthChanged;
            if (_gameManager != null)
            {
                _gameManager.OnLevelUp -= HandleUpgradeStart;
                if (UpgradeManager.Instance != null)
                {
                    UpgradeManager.Instance.OnFinishedSelecting -= HandleUpgradeFinish;
                }
            }
        }

        private void Update()
        {
            _timeSinceLastAttack += Time.deltaTime;
            UpdateStateTransitions();
            UpdateSleightCooldowns();
        }
        
        private void UpdateSleightCooldowns()
        {
            if (_isSleightOnCooldown)
            {
                _sleightCooldownTimer -= Time.deltaTime;
                if (_sleightCooldownTimer <= 0f)
                {
                    RollNextSleight();
                }
            }
        }
        
        private void RollNextSleight()
        {
            if (unlockedSleights == null || unlockedSleights.Count == 0) return;

            _isSleightOnCooldown = false;
            
            int randomIndex = UnityEngine.Random.Range(0, unlockedSleights.Count);
            currentStashedSleight = unlockedSleights[randomIndex];

            if (_rhythmInputHandler != null)
            {
                _rhythmInputHandler.ActiveSleight = currentStashedSleight;
            }
        }

        public void ChangeWeapon(WeaponSlot slot, Weapon newWeapon)
        {
            if (slot == WeaponSlot.Melee)
            {
                equippedMeleeWeapon = newWeapon;
            }
            else if (slot == WeaponSlot.Ranged)
            {
                equippedRangedWeapon = newWeapon;
            }
        }

        public void UnlockSleight(SleightType sleight)
        {
            if (!unlockedSleights.Contains(sleight))
            {
                unlockedSleights.Add(sleight);
                
                if (currentStashedSleight == SleightType.None && !_isSleightOnCooldown)
                {
                    RollNextSleight();
                }
            }
        }

        public void UpdateSleightWeapon(SleightType sleight, SleightWeapon newWeapon)
        {
            _sleightMap[sleight] = newWeapon;
        }

        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            if (currentHealth < _lastHealthAmount)
            {
                if (_rhythmInputHandler != null) _rhythmInputHandler.DropCombo();
            }
            _lastHealthAmount = currentHealth;
        }

        private void HandleUpgradeStart(int level) => _isUpgrading = true;
        private void HandleUpgradeFinish() => _isUpgrading = false;
        
        private void HandleDashStart() => _isDashing = true;
        private void HandleDashEnd() => _isDashing = false;

        private void UpdateStateTransitions()
        {
            if (_currentState == AttackState.Idle) return;

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
                if (_rhythmInputHandler != null) 
                {
                    isOnBeat = _rhythmInputHandler.ProcessInput(RhythmInputType.Melee);
                    if (_rhythmInputHandler.TriggeredSleight != SleightType.None) return;
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
                    isOnBeat = _rhythmInputHandler.ProcessInput(RhythmInputType.Ranged);
                    if (_rhythmInputHandler.TriggeredSleight != SleightType.None) return;
                }
                ExecuteAttack(isRanged: true, isOnBeat);
            }
        }

        bool CanAttack() 
        {
            if (_isUpgrading || _isDashing) return false;
            if (_currentState == AttackState.Idle) return true;
            return _timeSinceLastAttack >= attackCancelTime;
        }

        private void ExecuteAttack(bool isRanged, bool isOnBeat)
        {
            if (_currentState == AttackState.Special || _rhythmInputHandler == null) return;

            bool isFinisher = _rhythmInputHandler.IsFinisher;
            int attackIndex = Mathf.Max(0, _rhythmInputHandler.CurrentCombo - 1);

            string animToPlay = "";
            Weapon activeWeapon = isRanged ? equippedRangedWeapon : equippedMeleeWeapon;

            if (isRanged)
            {
                if (activeWeapon is RangedWeapon rWeapon) 
                {
                    animToPlay = isFinisher ? rWeapon.finisherAnimation : rWeapon.fireAnimation;
                }
            }
            else 
            {
                if (activeWeapon is MeleeWeapon mWeapon)
                {
                    if (isFinisher) 
                    {
                        animToPlay = mWeapon.finisherAnimation;
                    }
                    else if (mWeapon.comboAnimations != null && mWeapon.comboAnimations.Length > 0)
                    {
                        animToPlay = mWeapon.comboAnimations[attackIndex % mWeapon.comboAnimations.Length];
                    }
                }
            }

            if (activeWeapon != null && _sfxSource != null)
            {
                AudioClip clipToPlay = activeWeapon.attackSFX;
                if (clipToPlay != null)
                {
                    _sfxSource.PlayOneShot(clipToPlay, activeWeapon.soundVolume);
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

        public void ExecuteSpecialSleight(SleightType sleight)
        {
            if (!_sleightMap.TryGetValue(sleight, out SleightWeapon sleightWeapon)) return;

            _currentState = AttackState.Special;
            _timeSinceLastAttack = 0f;

            if (_playerMovement != null && !_isDashing) _playerMovement.EnterAttackState();
            
            if (_animator != null && !string.IsNullOrEmpty(sleightWeapon.animationName)) 
            {
                _animator.Play(sleightWeapon.animationName, 0, 0f);
            }

            if (_sfxSource != null && sleightWeapon.attackSFX != null)
            {
                _sfxSource.PlayOneShot(sleightWeapon.attackSFX, sleightWeapon.soundVolume);
            }

            sleightWeapon.ExecuteSleight(transform, _currentState);

            if (_rhythmInputHandler != null) 
            {
                _rhythmInputHandler.DropCombo();
                _rhythmInputHandler.ActiveSleight = SleightType.None;
            }
            
            currentStashedSleight = SleightType.None;
            _sleightCooldownTimer = sleightWeapon.cooldown;
            _isSleightOnCooldown = true;
        }

        public void CancelAttack()
        {
            if (_currentState == AttackState.Idle || _currentState == AttackState.Special) return;
            _currentState = AttackState.Idle;
            _animator.Play("Idle");
            
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