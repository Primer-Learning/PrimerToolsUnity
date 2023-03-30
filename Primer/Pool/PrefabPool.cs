using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Primer
{
    public class PrefabPool<T> : IPool<T> where T : Component
    {
        private readonly Queue<T> pool = new();
        private readonly WeakSet<T> inUse = new();

        private readonly string prefabName;
        private Object prefab;

        public PrefabPool(string prefabName)
        {
            this.prefabName = prefabName;
        }

        public void Fill(int amount)
        {
            while (pool.Count < amount)
                pool.Enqueue(Create(null));
        }

        public T Get(Transform parent = null)
        {
            var target = Find(parent);
            var go = target.gameObject;

            if (target.transform.parent != parent)
                target.transform.SetParent(parent, worldPositionStays: true);

            PoolOrchestrator.Reuse(go);

            if (target is IPoolable poolable)
                poolable.OnReuse();

            inUse.Add(target);
            return target;
        }

        private T Find(Transform parent)
        {
            T result;

            do {
                result = pool.Count != 0
                    ? pool.Dequeue()
                    : PoolOrchestrator.FindPrefab<T>(prefabName) ?? Create(parent);
            } while (result == null);

            return result;
        }

        public void Recycle(T target)
        {
            if (target == null)
                return;

            PoolOrchestrator.Recycle(target.gameObject);

            if (target is IPoolable poolable)
                poolable.OnRecycle();

            inUse.Remove(target);
            pool.Enqueue(target);
        }

        public void RecycleAll()
        {
            foreach (var target in inUse) {
                Recycle(target);
            }
        }

        protected T Create(Transform parent)
        {
            prefab ??= GetPrefab(prefabName);
            return PoolOrchestrator.CreatePrefab<T>(prefabName, prefab, parent);
        }

        private static Object GetPrefab(string prefabName)
        {
            var prefab = Resources.Load(prefabName);

            if (prefab is null) {
                throw new Exception($"Cannot find prefab {prefabName}");
            }

            return prefab;
        }
    }
}
