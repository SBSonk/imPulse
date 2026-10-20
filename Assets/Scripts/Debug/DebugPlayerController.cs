using UnityEngine;
using UnityEngine.InputSystem;
using ImPulse.Core;

namespace ImPulse.Debug
{
    public class DebugPlayerController : MonoBehaviour
    {
        [Header("References")]
        public RhythmInputHandler rhythmInput;

        [Header("Inputs")]
        // Defining bindings directly in code so it works immediately out of the box
        public InputAction dodgeAction = new InputAction("Dodge", binding: "<Keyboard>/space");
        public InputAction closeAttackAction = new InputAction("CloseAttack", binding: "<Keyboard>/j");
        public InputAction rangeAttackAction = new InputAction("RangeAttack", binding: "<Keyboard>/k");

        private void OnEnable()
        {
            // New Input System requires actions to be explicitly enabled
            dodgeAction.Enable();
            closeAttackAction.Enable();
            rangeAttackAction.Enable();
        }

        private void OnDisable()
        {
            dodgeAction.Disable();
            closeAttackAction.Disable();
            rangeAttackAction.Disable();
        }

        // private void Update()
        // {
        //     // Read the inputs and pass the enum to the validator
        //     if (dodgeAction.WasPressedThisFrame())
        //     {
        //         rhythmInput.ProcessAction(RhythmActionType.Dodge);
        //     }
        //     else if (closeAttackAction.WasPressedThisFrame())
        //     {
        //         rhythmInput.ProcessAction(RhythmActionType.CloseAttack);
        //     }
        //     else if (rangeAttackAction.WasPressedThisFrame())
        //     {
        //         rhythmInput.ProcessAction(RhythmActionType.RangeAttack);
        //     }
        // }
    }
}