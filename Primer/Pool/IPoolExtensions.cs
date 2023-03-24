using System;
using UnityEngine;

namespace Primer
{
    public static class IPoolExtensions
    {
        public static IPool<T> Specialize<T>(this IPool<T> self, Action<T> onUse = null, Action<T> onRecycle = null)
            where T : Component
        {
            return new SpecializedPool<T>(self) {
                onUse = onUse,
                onRecycle = onRecycle,
            };
        }
    }
}
