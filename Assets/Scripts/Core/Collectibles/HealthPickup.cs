using UnityEngine;
using ImPulse.Core;

namespace ImPulse.Collectibles
{
    public class HealthPickup : Pickup
    {
        public float healAmount = 20f;

        protected override void Collect(GameObject player)
        {
            if (player.TryGetComponent<HasHealth>(out var health))
            {
                health.Heal(healAmount);
            }
        }
    }
}