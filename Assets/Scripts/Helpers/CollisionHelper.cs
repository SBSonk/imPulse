using UnityEngine;

namespace ImPulse.Helpers
{
    public static class CollisionHelper
    {
        public static bool CircleToCircle(Vector3 posA, float radiusA, Vector3 posB, float radiusB)
        {
            float dx = posA.x - posB.x;
            float dy = posA.y - posB.y;
            float radSum = radiusA + radiusB;
            
            return (dx * dx + dy * dy) <= (radSum * radSum);
        }

        public static bool BoxToBox(Vector3 posA, float radiusA, Vector3 posB, float radiusB)
        {
            return Mathf.Abs(posA.x - posB.x) <= (radiusA + radiusB) &&
                   Mathf.Abs(posA.y - posB.y) <= (radiusA + radiusB);
        }

        public static bool BoxToCircle(Vector3 boxPos, float boxRadius, Vector3 circlePos, float circleRadius)
        {
            float closestX = Mathf.Clamp(circlePos.x, boxPos.x - boxRadius, boxPos.x + boxRadius);
            float closestY = Mathf.Clamp(circlePos.y, boxPos.y - boxRadius, boxPos.y + boxRadius);

            float dx = circlePos.x - closestX;
            float dy = circlePos.y - closestY;

            return (dx * dx + dy * dy) <= (circleRadius * circleRadius);
        }
    }
}