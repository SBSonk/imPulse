using UnityEngine;
using UnityEngine.InputSystem;

namespace ImPulse.UI
{
    public class MouseParallaxUI : MonoBehaviour
    {
        [System.Serializable]
        public class ParallaxLayer
        {
            public RectTransform target;
            
            [Header("Multipliers")]
            [Tooltip("How far the UI moves on X and Y axes.")]
            public Vector2 moveMultiplier = new Vector2(15f, 15f);
            
            [Tooltip("How much the UI tilts on X and Y axes.")]
            public Vector2 rotateMultiplier = new Vector2(5f, 5f);
            
            [Tooltip("Inverts the movement and rotation direction.")]
            public bool invertDirection = true;

            [HideInInspector] public Vector2 startPosition;
            [HideInInspector] public Quaternion startRotation;
        }

        [Header("Parallax Layers")]
        public ParallaxLayer mainLayer;
        public ParallaxLayer foregroundLayer;

        [Header("Global Settings")]
        [Tooltip("How quickly the UI lerps to the target position and rotation.")]
        [SerializeField] private float smoothSpeed = 10f;

        private void Awake()
        {
            InitializeLayer(mainLayer);
            InitializeLayer(foregroundLayer);
        }

        private void InitializeLayer(ParallaxLayer layer)
        {
            if (layer != null && layer.target != null)
            {
                layer.startPosition = layer.target.anchoredPosition;
                layer.startRotation = layer.target.localRotation;
            }
        }

        private void Update()
        {
            Vector2 mousePos = Vector2.zero;
            
            if (Mouse.current != null)
            {
                mousePos = Mouse.current.position.ReadValue();
            }

            // Normalize the mouse position from -1 (left/bottom edge) to 1 (right/top edge)
            float normalizedX = (mousePos.x - (Screen.width * 0.5f)) / (Screen.width * 0.5f);
            float normalizedY = (mousePos.y - (Screen.height * 0.5f)) / (Screen.height * 0.5f);

            // Clamp just in case the mouse goes out of the game window bounds
            normalizedX = Mathf.Clamp(normalizedX, -1f, 1f);
            normalizedY = Mathf.Clamp(normalizedY, -1f, 1f);

            UpdateLayer(mainLayer, normalizedX, normalizedY);
            UpdateLayer(foregroundLayer, normalizedX, normalizedY);
        }

        private void UpdateLayer(ParallaxLayer layer, float normX, float normY)
        {
            if (layer == null || layer.target == null) return;

            float dir = layer.invertDirection ? -1f : 1f;

            // Position Offset
            Vector2 targetOffset = new Vector2(normX * layer.moveMultiplier.x, normY * layer.moveMultiplier.y) * dir;
            Vector2 targetPosition = layer.startPosition + targetOffset;

            // Rotation Offset (Tilt)
            // Tilting on X axis (up/down) based on Y mouse movement
            // Tilting on Y axis (left/right) based on X mouse movement
            float targetRotX = normY * layer.rotateMultiplier.x * -dir; 
            float targetRotY = normX * layer.rotateMultiplier.y * dir;
            
            Quaternion targetRotation = layer.startRotation * Quaternion.Euler(targetRotX, targetRotY, 0f);

            // Use unscaledDeltaTime so the UI still moves interactively when the game is paused
            layer.target.anchoredPosition = Vector2.Lerp(
                layer.target.anchoredPosition, 
                targetPosition, 
                Time.unscaledDeltaTime * smoothSpeed
            );

            layer.target.localRotation = Quaternion.Lerp(
                layer.target.localRotation,
                targetRotation,
                Time.unscaledDeltaTime * smoothSpeed
            );
        }
    }
}