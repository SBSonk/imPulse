using UnityEngine;
using ImPulse.Core;

namespace ImPulse.Collectibles
{
    public class XPPickup : Pickup
    {
        private int _xpAmount;

        public void Initialize(int amount)
        {
            _xpAmount = amount;
        }

        protected override void Collect(GameObject player)
        {
            var gm = FindAnyObjectByType<GameManager>();
            if (gm != null)
            {
                gm.AddXP(_xpAmount);
            }
        }
    }
}