using System;
using UnityEngine;

namespace Primer
{
    internal class SpecializedPool<T> : IPool<T> where T : Component
    {
        public Action<T> onRecycle { init; get; }
        public Action<T> onUse { init; get; }

        private readonly IPool<T> basePool;
        private readonly WeakSet<T> inUse = new();

        public SpecializedPool(IPool<T> basePool)
        {
            this.basePool = basePool;
        }

        public T Get(Transform parent = null)
        {
            var component = basePool.Get(parent);
            onUse?.Invoke(component);
            inUse.Add(component);
            return component;
        }

        public void Recycle(T target)
        {
            if (target == null)
                return;

            inUse.Remove(target);
            basePool.Recycle(target);
            onRecycle?.Invoke(target);
        }

        public void RecycleAll()
        {
            foreach (var target in inUse) {
                Recycle(target);
            }
        }
    }
}
