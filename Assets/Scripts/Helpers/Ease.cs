using UnityEngine;

namespace ImPulse.Helpers
{
    public enum EaseType
    {
        Linear,
        InSine, OutSine, InOutSine,
        InQuad, OutQuad, InOutQuad,
        InCubic, OutCubic, InOutCubic,
        InQuart, OutQuart, InOutQuart,
        InQuint, OutQuint, InOutQuint,
        InExpo, OutExpo, InOutExpo,
        InBack, OutBack, InOutBack,
        InElastic, OutElastic, InOutElastic,
        InBounce, OutBounce, InOutBounce
    }

    public static class Easing
    {
        public static float Evaluate(EaseType easeType, float t) => easeType switch
        {
            EaseType.Linear => Linear(t),
            
            EaseType.InSine => InSine(t),
            EaseType.OutSine => OutSine(t),
            EaseType.InOutSine => InOutSine(t),
            
            EaseType.InQuad => InQuad(t),
            EaseType.OutQuad => OutQuad(t),
            EaseType.InOutQuad => InOutQuad(t),
            
            EaseType.InCubic => InCubic(t),
            EaseType.OutCubic => OutCubic(t),
            EaseType.InOutCubic => InOutCubic(t),
            
            EaseType.InQuart => InQuart(t),
            EaseType.OutQuart => OutQuart(t),
            EaseType.InOutQuart => InOutQuart(t),
            
            EaseType.InQuint => InQuint(t),
            EaseType.OutQuint => OutQuint(t),
            EaseType.InOutQuint => InOutQuint(t),
            
            EaseType.InExpo => InExpo(t),
            EaseType.OutExpo => OutExpo(t),
            EaseType.InOutExpo => InOutExpo(t),
            
            EaseType.InBack => InBack(t),
            EaseType.OutBack => OutBack(t),
            EaseType.InOutBack => InOutBack(t),
            
            EaseType.InElastic => InElastic(t),
            EaseType.OutElastic => OutElastic(t),
            EaseType.InOutElastic => InOutElastic(t),
            
            EaseType.InBounce => InBounce(t),
            EaseType.OutBounce => OutBounce(t),
            EaseType.InOutBounce => InOutBounce(t),
            
            _ => t
        };

        public static float Linear(float t) => t;

        public static float InSine(float t) => 1f - Mathf.Cos((t * Mathf.PI) / 2f);
        public static float OutSine(float t) => Mathf.Sin((t * Mathf.PI) / 2f);
        public static float InOutSine(float t) => -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;

        public static float InQuad(float t) => t * t;
        public static float OutQuad(float t) => 1f - (1f - t) * (1f - t);
        public static float InOutQuad(float t) => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

        public static float InCubic(float t) => t * t * t;
        public static float OutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
        public static float InOutCubic(float t) => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;

        public static float InQuart(float t) => t * t * t * t;
        public static float OutQuart(float t) => 1f - Mathf.Pow(1f - t, 4f);
        public static float InOutQuart(float t) => t < 0.5f ? 8f * t * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 4f) / 2f;

        public static float InQuint(float t) => t * t * t * t * t;
        public static float OutQuint(float t) => 1f - Mathf.Pow(1f - t, 5f);
        public static float InOutQuint(float t) => t < 0.5f ? 16f * t * t * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 5f) / 2f;

        public static float InExpo(float t) => t == 0f ? 0f : Mathf.Pow(2f, 10f * t - 10f);
        public static float OutExpo(float t) => t == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);
        public static float InOutExpo(float t)
        {
            if (t == 0f) return 0f;
            if (t == 1f) return 1f;
            return t < 0.5f ? Mathf.Pow(2f, 20f * t - 10f) / 2f : (2f - Mathf.Pow(2f, -20f * t + 10f)) / 2f;
        }

        public static float InBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return c3 * t * t * t - c1 * t * t;
        }

        public static float OutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        public static float InOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;
            return t < 0.5f
                ? (Mathf.Pow(2f * t, 2f) * ((c2 + 1f) * 2f * t - c2)) / 2f
                : (Mathf.Pow(2f * t - 2f, 2f) * ((c2 + 1f) * (t * 2f - 2f) + c2) + 2f) / 2f;
        }

        public static float InElastic(float t)
        {
            const float c4 = (2f * Mathf.PI) / 3f;
            if (t == 0f) return 0f;
            if (t == 1f) return 1f;
            return -Mathf.Pow(2f, 10f * t - 10f) * Mathf.Sin((t * 10f - 10.75f) * c4);
        }

        public static float OutElastic(float t)
        {
            const float c4 = (2f * Mathf.PI) / 3f;
            if (t == 0f) return 0f;
            if (t == 1f) return 1f;
            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
        }

        public static float InOutElastic(float t)
        {
            const float c5 = (2f * Mathf.PI) / 4.5f;
            if (t == 0f) return 0f;
            if (t == 1f) return 1f;
            return t < 0.5f
                ? -(Mathf.Pow(2f, 20f * t - 10f) * Mathf.Sin((20f * t - 11.125f) * c5)) / 2f
                : (Mathf.Pow(2f, -20f * t + 10f) * Mathf.Sin((20f * t - 11.125f) * c5)) / 2f + 1f;
        }

        public static float OutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1f / d1)
            {
                return n1 * t * t;
            }
            if (t < 2f / d1)
            {
                return n1 * (t -= 1.5f / d1) * t + 0.75f;
            }
            if (t < 2.5f / d1)
            {
                return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            }
            return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }

        public static float InBounce(float t) => 1f - OutBounce(1f - t);

        public static float InOutBounce(float t) => t < 0.5f
            ? (1f - OutBounce(1f - 2f * t)) / 2f
            : (1f + OutBounce(2f * t - 1f)) / 2f;
    }
}