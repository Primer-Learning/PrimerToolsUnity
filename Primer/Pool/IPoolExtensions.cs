using System;
using UnityEngine;

namespace Primer
{
    public static class IPoolExtensions
    {
        public static IPool<T> Specialize<T>(this IPool<T> self, Action<T> onRecycle = null, Action<T> onUse = null)
            where T : Component
        {
            return new SpecializedPool<T>(self) {
                onRecycle = onRecycle,
                onUse = onUse,
            };
        }

        public static IPool<T> ForTimeline<T>(this IPool<T> self) where T : Component
        {
            return new SpecializedPool<T>(self) {
                onUse = t => {
                    t.GetComponent<PoolData>().isInTimeline = true;
                    t.hideFlags = HideFlags.DontSave;
                },
                onRecycle = t => {
                    t.GetComponent<PoolData>().isInTimeline = false;
                    t.hideFlags = HideFlags.None;
                },
            };
        }
    }
}
