using UnityEngine;

namespace Primer
{
    public static class VectorExtensions
    {
        public static Vector3 InvertScale(this Vector3 v) => new(1 / v.x, 1 / v.y, 1 / v.z);

        public static float Average(this Vector3 v) => (v.x + v.y + v.z) / 3;

        public static Vector3 ScaleBy(this Vector3 v, Vector3 scale)
        {
            return new Vector3(v.x * scale.x, v.y * scale.y, v.z * scale.z);
        }
    }
}
