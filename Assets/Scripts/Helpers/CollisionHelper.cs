using UnityEngine;

namespace ImPulse.Helpers
{
    public static class CollisionHelper
    {
        public static bool CircleToCircle(Vector3 posA, float radiusA, Vector3 posB, float radiusB)
        {
            float dx = posA.x - posB.x;
            float dz = posA.z - posB.z;
            float radSum = radiusA + radiusB;
            
            return (dx * dx + dz * dz) <= (radSum * radSum);
        }

        public static bool BoxToBox(Vector3 posA, float radiusA, Vector3 posB, float radiusB)
        {
            return Mathf.Abs(posA.x - posB.x) <= (radiusA + radiusB) &&
                   Mathf.Abs(posA.z - posB.z) <= (radiusA + radiusB);
        }

        public static bool BoxToCircle(Vector3 boxPos, float boxRadius, Vector3 circlePos, float circleRadius)
        {
            float closestX = Mathf.Clamp(circlePos.x, boxPos.x - boxRadius, boxPos.x + boxRadius);
            float closestZ = Mathf.Clamp(circlePos.z, boxPos.z - boxRadius, boxPos.z + boxRadius);

            float dx = circlePos.x - closestX;
            float dz = circlePos.z - closestZ;

            return (dx * dx + dz * dz) <= (circleRadius * circleRadius);
        }
    }
}