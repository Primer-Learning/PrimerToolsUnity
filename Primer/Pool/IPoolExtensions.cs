using System;
using UnityEngine;

namespace Primer
{
    public static class IPoolExtensions
    {
        public static T Get<T>(this IPool<T> self, string name) where T : Component
        {
            var target = self.Get();
            target.name = name;
            return target;
        }

        public static IPool<T> Specialize<T>(this IPool<T> self, Action<T> onUse = null, Action<T> onRecycle = null)
            where T : Component
        {
            return new SpecializedPool<T>(self) {
                onUse = onUse,
                onRecycle = onRecycle,
            };
        }

        public static IPool<TPooled> Convert<TInternal, TPooled>(
            this IPool<TInternal> self,
            Func<TInternal, TPooled> onUse,
            Func<TPooled, TInternal> onRecycle
        )
            where TPooled : Component
            where TInternal : Component
        {
            return new PoolConverter<TInternal, TPooled>(self, onUse, onRecycle);
        }
    }
}
