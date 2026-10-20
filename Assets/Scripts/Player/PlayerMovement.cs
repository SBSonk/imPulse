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
        InAttack
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

        [Header("Attack Movement Settings")]
        [SerializeField] float attackControlRegenDuration = 0.4f;
        [SerializeField] EaseType attackEaseType = EaseType.InQuad;

        [Header("State")]
        [SerializeField] MovementState _currentState = MovementState.Idle;
        [SerializeField] Vector3 _currentVelocity;

        RhythmInputHandler _rhythmInputHandler;

        public Action OnDashStart;

        private Vector3 _currentInputDirection;
        private Vector3 _dashDirection;
        private float _dashTimer;
        
        private Vector3 _attackStartVelocity;
        private float _attackTimer;

        private bool _dashTriggered;
        private bool _hasStateControl = true;

        void Start()
        {
            _rhythmInputHandler = FindAnyObjectByType<RhythmInputHandler>();
        }

        private void Update()
        {
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
            if (value.isPressed && _currentInputDirection.sqrMagnitude > 0.01f)
            {
                _dashTriggered = true;
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
                    if (_currentInputDirection.sqrMagnitude > 0.01f)
                    {
                        _currentState = MovementState.Moving;
                    }
                    break;

                case MovementState.Moving:
                    if (_currentInputDirection.sqrMagnitude <= 0.01f)
                    {
                        _currentState = MovementState.Idle;
                    }
                    break;

                case MovementState.Dashing:
                    if (_dashTimer >= dashDuration)
                    {
                        _currentState = _currentInputDirection.sqrMagnitude > 0.01f ? MovementState.Moving : MovementState.Idle;
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

                    if (_currentInputDirection.sqrMagnitude > 0.01f && attackEase > .9f)
                    {
                        FaceMoveDirection(_currentInputDirection);
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

            OnDashStart?.Invoke();

            _rhythmInputHandler.ProcessInput();
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

        // funny quake accel
        void Accelerate(Vector3 wishDir, float wishSpeed, float accel)
        {
            float currentSpeed = Vector3.Dot(_currentVelocity, wishDir);
            float addSpeed = wishSpeed - currentSpeed;
            
            if (addSpeed <= 0f) return;

            float accelAmount = accel * wishSpeed * Time.deltaTime;

            if (accelAmount > addSpeed)
            {
                accelAmount = addSpeed;
            }

            _currentVelocity += wishDir * accelAmount;
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
            _currentState = MovementState.Idle; 
        }
    }
}