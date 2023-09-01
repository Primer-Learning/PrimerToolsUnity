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
            if (typeof(T) == typeof(float) || typeof(T) == typeof(int) || typeof(T) == typeof(double))
                return typeof(Mathf).GetMethod("Lerp");

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
    }
}
