using UnityEngine;
using UnityEngine.UI;

namespace ImPulse.UI
{
    public class SleightInputIconUI : MonoBehaviour
    {
        [Header("References")]
        public Image iconImage;

        [Header("Color Settings")]
        public Color pendingColor = new Color(1f, 1f, 1f, 0.3f);
        public Color correctColor = new Color(1f, 1f, 1f, 1f);
        public Color incorrectColor = new Color(1f, 0f, 0f, 1f);
        public Color successColor = new Color(0f, 1f, 0f, 1f);

        public void Initialize(Sprite sprite)
        {
            if (iconImage != null)
            {
                iconImage.sprite = sprite;
                ResetIcon();
            }
        }

        public void ResetIcon()
        {
            SetColor(pendingColor);
        }

        public void SetCorrect()
        {
            SetColor(correctColor);
        }

        public void SetIncorrect()
        {
            SetColor(incorrectColor);
        }

        public void SetSuccess()
        {
            SetColor(successColor);
        }

        private void SetColor(Color color)
        {
            if (iconImage != null) iconImage.color = color;
        }
    }
}