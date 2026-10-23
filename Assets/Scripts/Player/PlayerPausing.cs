using UnityEngine;
using UnityEngine.InputSystem;
using ImPulse.Core;

namespace ImPulse.Player
{
    public class PlayerPausing : MonoBehaviour
    {
        public void OnPause(InputValue value) 
        {
            if (value.isPressed && GameManager.Instance != null)
            {
                GameManager.Instance.TogglePause();
            }
        }
    }
}