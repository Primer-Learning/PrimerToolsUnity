using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Primer.Animation
{
    public static class LerpMethods
    {
        private static readonly Dictionary<Type, MethodInfo> lerpMethods = new();

        public static Func<T, T, float, T> GetLerpMethod<T>()
        {
            if (!lerpMethods.TryGetValue(typeof(T), out var method)) {
                method = GetNewLerpMethod<T>();
                lerpMethods.Add(typeof(T), method);
            }

            return (Func<T, T, float, T>)method.CreateDelegate(typeof(Func<T, T, float, T>));
        }

        private static MethodInfo GetNewLerpMethod<T>()
        {
            if (typeof(T) == typeof(float) || typeof(T) == typeof(double))
                return typeof(Mathf).GetMethod("Lerp");

            if (typeof(T) == typeof(int))
                return typeof(LerpMethods).GetMethod("IntLerp");

            if (typeof(T) == typeof(Vector2Int))
                return typeof(LerpMethods).GetMethod("Vector2IntLerp");

            // Special case for Quaternion so we use Slerp instead of Lerp
            if (typeof(T) == typeof(Quaternion))
                return typeof(Quaternion).GetMethod("Slerp");

            if (typeof(T) == typeof(Color))
            {
                return typeof(PrimerColor).GetMethod("ModeratelyJuicyInterpolate");
            }
            
            var lerp = typeof(T).GetMethod("Lerp");

            if (lerp is null) {
                throw new ArgumentException($"{nameof(Tween)}() couldn't find static .Lerp() in {typeof(T).FullName}");
            }

            return lerp;
        }
        
        public static int IntLerp(int a, int b, float t)
        {
            return (int)Math.Round(a + (b - a) * t);
        }
        
        // Lerp for Vector2Int
        public static Vector2Int Vector2IntLerp(Vector2Int a, Vector2Int b, float t)
        {
            return new Vector2Int(
                IntLerp(a.x, b.x, t),
                IntLerp(a.y, b.y, t)
            );
        }
    }
}
