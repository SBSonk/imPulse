using UnityEngine;
using ImPulse.Core;

namespace ImPulse.Player
{
    [RequireComponent(typeof(HasHealth))]
    public class PlayerDeathHandler : MonoBehaviour
    {
        private HasHealth _playerHealth;

        private void Awake()
        {
            _playerHealth = GetComponent<HasHealth>();
            _playerHealth.OnDeath += HandlePlayerDeath;
        }

        private void OnDestroy()
        {
            if (_playerHealth != null)
            {
                _playerHealth.OnDeath -= HandlePlayerDeath;
            }
        }

        private void HandlePlayerDeath()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }
    }
}