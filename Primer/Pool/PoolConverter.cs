using System;
using UnityEngine;

namespace Primer
{
    internal class PoolConverter<TInternal, TPooled> : IPool<TPooled>
        where TPooled : Component
        where TInternal : Component
    {
        public Func<TPooled, TInternal> onRecycle { get; }
        public Func<TInternal, TPooled> onUse { get; }

        private readonly IPool<TInternal> basePool;
        private readonly WeakSet<TPooled> inUse = new();

        public PoolConverter(
            IPool<TInternal> basePool,
            Func<TInternal, TPooled> onUse,
            Func<TPooled, TInternal> onRecycle
        ) {
            this.basePool = basePool;
            this.onRecycle = onRecycle;
            this.onUse = onUse;
        }

        public TPooled Get(Transform parent = null)
        {
            var component = basePool.Get(parent);
            var result = onUse.Invoke(component);
            inUse.Add(result);
            return result;
        }

        public void Recycle(TPooled target)
        {
            if (target == null)
                return;

            inUse.Remove(target);
            var pooled = onRecycle.Invoke(target);
            basePool.Recycle(pooled);
        }

        public void RecycleAll()
        {
            foreach (var target in inUse) {
                Recycle(target);
            }
        }

        public void Fill(int amount)
        {
            basePool.Fill(amount);
        }
    }
}
