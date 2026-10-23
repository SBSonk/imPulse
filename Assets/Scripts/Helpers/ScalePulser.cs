using UnityEngine;

namespace ImPulse.Helpers
{
    public class ScalePulser : MonoBehaviour
    {
        [Header("Pulse Settings")]
        public Vector3 minScale = Vector3.one;
        
        public Vector3 maxScale = new Vector3(1.2f, 1.2f, 1.2f);
        
        [Tooltip("How fast the object pulses.")]
        public float speed = 5f;

        private void Update()
        {
            float t = (Mathf.Sin(Time.unscaledTime * speed) + 1f) / 2f;
            transform.localScale = Vector3.Lerp(minScale, maxScale, t);
        }
    }
}