using UnityEngine;
using UnityEngine.InputSystem;
using ImPulse.Helpers;
using System;
using ImPulse.Core;

namespace ImPulse.Player
{
    public enum MovementState
    {
        Idle,
        Moving,
        Dashing, 
        InAttack,
        Stunned
    }

    [RequireComponent(typeof(PlayerInput))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float accelerationRate = 10f; 
        [SerializeField] float frictionRate = 8f;
        [SerializeField] float rotateSpeed = 50f;

        [Header("Dash Settings")]
        [SerializeField] float dashInitialSpeed = 15f;
        [SerializeField] float dashDuration = 0.4f;
        [SerializeField] EaseType dashEaseType = EaseType.OutQuad;

        [Header("Stamina Settings")]
        [SerializeField] float maxStamina = 2.0f; 
        [SerializeField] float staminaRegenRate = 0.5f; 
        [SerializeField] float dashStaminaCost = 1.0f;

        [Header("Attack Movement Settings")]
        [SerializeField] float attackControlRegenDuration = 0.4f;
        [SerializeField] EaseType attackEaseType = EaseType.InQuad;

        [Header("Knockback Settings")]
        [SerializeField] EaseType knockbackEaseType = EaseType.OutCubic;

        [Header("State")]
        [SerializeField] MovementState _currentState = MovementState.Idle;
        [SerializeField] Vector3 _currentVelocity;

        RhythmInputHandler _rhythmInputHandler;
        PlayerStatus _playerStatus;
        GameManager _gameManager;

        public Action OnDashStart;
        public Action OnDashEnd; 

        private Vector3 _currentInputDirection;
        private Vector3 _dashDirection;
        private float _dashTimer;
        
        private float _currentStamina;
        private float _baseMaxStamina; 
        
        private Vector3 _attackStartVelocity;
        private float _attackTimer;

        private Vector3 _knockbackStartVelocity;
        private float _stunDuration;
        private float _stunTimer;

        private bool _dashTriggered;
        private bool _hasStateControl = true;
        
        private bool _isUpgrading = false;

        void Start()
        {
            _rhythmInputHandler = FindAnyObjectByType<RhythmInputHandler>();
            _playerStatus = GetComponent<PlayerStatus>();
            
            _baseMaxStamina = maxStamina;
            _currentStamina = maxStamina;

            _gameManager = FindAnyObjectByType<GameManager>();
            if (_gameManager != null)
            {
                _gameManager.OnLevelUp += HandleUpgradeStart;
                UpgradeManager.Instance.OnFinishedSelecting += HandleUpgradeFinish;
            }
        }

        void OnDestroy()
        {
            if (_gameManager != null)
            {
                _gameManager.OnLevelUp -= HandleUpgradeStart;
                UpgradeManager.Instance.OnFinishedSelecting -= HandleUpgradeFinish;
            }
        }

        private void HandleUpgradeStart(int level) => _isUpgrading = true;
        private void HandleUpgradeFinish() => _isUpgrading = false;

        private void Update()
        {
            if (_currentStamina < maxStamina)
            {
                _currentStamina = Mathf.Clamp(_currentStamina + staminaRegenRate * Time.deltaTime, 0f, maxStamina);
            }

            UpdateStateTransitions();
            ExecuteCurrentState();

            transform.Translate(_currentVelocity * Time.deltaTime, Space.World);

            _dashTriggered = false;
        }

        public void OnMove(InputValue value)
        {
            Vector2 input2D = value.Get<Vector2>().normalized;
            _currentInputDirection = new Vector3(input2D.x, input2D.y);
        }

        public void OnDash(InputValue value)
        {
            if (_isUpgrading) return;

            if (value.isPressed && _currentInputDirection.sqrMagnitude > 0.01f && _currentState != MovementState.Stunned)
            {
                if (_currentStamina >= dashStaminaCost)
                {
                    _dashTriggered = true;
                }
            }
        }

        private void UpdateStateTransitions()
        {
            if (_dashTriggered)
            {
                StartDash();
                return; 
            }

            if (!_hasStateControl) return;

            switch (_currentState)
            {
                case MovementState.Idle:
                    if (_currentInputDirection.sqrMagnitude > 0.01f) _currentState = MovementState.Moving;
                break;

                case MovementState.Moving:
                    if (_currentInputDirection.sqrMagnitude <= 0.01f) _currentState = MovementState.Idle;
                break;

                case MovementState.Dashing:
                    if (_dashTimer >= dashDuration)
                    {
                        _currentState = _currentInputDirection.sqrMagnitude > 0.01f ? MovementState.Moving : MovementState.Idle;
                        OnDashEnd?.Invoke(); 
                    }
                break;
            }
        }

        private void ExecuteCurrentState()
        {
            switch (_currentState)
            {
                case MovementState.Idle:
                    ApplyFriction(frictionRate);
                break;

                case MovementState.Moving:
                    ApplyFriction(frictionRate);
                    Accelerate(_currentInputDirection, moveSpeed, accelerationRate);

                    FaceMoveDirection(_currentInputDirection);
                break;

                case MovementState.Dashing:
                    _dashTimer += Time.deltaTime;

                    float dashNormTime = Mathf.Clamp01(_dashTimer / dashDuration);
                    float dashEase = Easing.Evaluate(dashEaseType, dashNormTime);

                    Vector3 pureDashVelocity = _dashDirection * dashInitialSpeed;
                    Vector3 targetNormalVelocity = _currentInputDirection * moveSpeed;

                    _currentVelocity = Vector3.Lerp(pureDashVelocity, targetNormalVelocity, dashEase);

                    FaceMoveDirection(_dashDirection);
                break;

                case MovementState.InAttack:
                    _attackTimer += Time.deltaTime;

                    float attackNormTime = Mathf.Clamp01(_attackTimer / attackControlRegenDuration);
                    float attackEase = Easing.Evaluate(attackEaseType, attackNormTime);

                    Vector3 frozenVelocity = Vector3.Lerp(_attackStartVelocity, Vector3.zero, attackEase);
                    Vector3 inputTargetVelocity = _currentInputDirection * moveSpeed;

                    _currentVelocity = Vector3.Lerp(frozenVelocity, inputTargetVelocity, attackEase);

                    if (_currentInputDirection.sqrMagnitude > 0.01f && attackEase > .9f) FaceMoveDirection(_currentInputDirection);
                break;

                case MovementState.Stunned:
                    _stunTimer += Time.deltaTime;

                    float stunNormTime = Mathf.Clamp01(_stunTimer / _stunDuration);
                    float stunEase = Easing.Evaluate(knockbackEaseType, stunNormTime);

                    Vector3 baseVelocity = _currentInputDirection * moveSpeed;
                    _currentVelocity = Vector3.Lerp(_knockbackStartVelocity, baseVelocity, stunEase);

                    if (_stunTimer >= _stunDuration)
                    {
                        _hasStateControl = true;
                        _currentState = _currentInputDirection.sqrMagnitude > 0.01f ? MovementState.Moving : MovementState.Idle;
                    }
                break;
            }
        }

        void FaceMoveDirection(Vector3 dir)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle), rotateSpeed * Time.deltaTime);
        }

        void StartDash()
        {
            if (_currentState == MovementState.InAttack) FreeAttackState();

            _currentState = MovementState.Dashing;
            _dashTimer = 0f;
            _dashDirection = _currentInputDirection;
            
            _currentStamina -= dashStaminaCost;

            if (_playerStatus != null) _playerStatus.GrantInvincibility(dashDuration);

            OnDashStart?.Invoke();

            if (_rhythmInputHandler != null) _rhythmInputHandler.ProcessInput(RhythmInputType.Dash);
        }

        void ApplyFriction(float friction)
        {
            float speed = _currentVelocity.magnitude;
            
            if (speed < 0.01f)
            {
                _currentVelocity = Vector3.zero;
                return;
            }

            float stopSpeed = 1.0f; 
            float control = (speed < stopSpeed) ? stopSpeed : speed;
            float drop = control * friction * Time.deltaTime;

            float newSpeed = speed - drop;
            if (newSpeed < 0) newSpeed = 0f;

            newSpeed /= speed;
            _currentVelocity *= newSpeed;
        }

        void Accelerate(Vector3 wishDir, float wishSpeed, float accel)
        {
            float currentSpeed = Vector3.Dot(_currentVelocity, wishDir);
            float addSpeed = wishSpeed - currentSpeed;
            
            if (addSpeed <= 0f) return;

            float accelAmount = accel * wishSpeed * Time.deltaTime;

            if (accelAmount > addSpeed) accelAmount = addSpeed;

            _currentVelocity += wishDir * accelAmount;
        }

        public void ApplyKnockback(Vector3 direction, float force, float duration)
        {
            if (_currentState == MovementState.Dashing) return; 

            _currentState = MovementState.Stunned;
            _hasStateControl = false;
            
            _knockbackStartVelocity = direction.normalized * force;
            _stunDuration = duration;
            _stunTimer = 0f;

            if (_currentState == MovementState.InAttack) FreeAttackState();
        }

        public void EnterAttackState()
        {
            _currentState = MovementState.InAttack;
            _hasStateControl = false;
            
            _attackStartVelocity = _currentVelocity;
            _attackTimer = 0f;
        }

        public void FreeAttackState()
        {
            _hasStateControl = true;
        }

        // --- UPGRADE API --- //
        public void SetBonusMaxStamina(float bonus)
        {
            maxStamina = _baseMaxStamina + bonus;
        }

        public void SetStaminaRegenRate(float newRate)
        {
            staminaRegenRate = newRate;
        }
        
        public float GetCurrentStamina() => _currentStamina;
        public float GetMaxStamina() => maxStamina;

        public Vector3 GetVelocity() => _currentVelocity;
    }
}