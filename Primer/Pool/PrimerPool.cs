using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Primer
{
    public class PrimerPool<T> : IPool<T> where T : Component
    {
        private readonly Queue<T> pool = new();

        private readonly string prefabName;
        private Object prefab;
        private bool hasCollectedOrphans;

        public PrimerPool(string prefabName)
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

            go.SetActive(true);
            PoolOrchestrator.Reuse(go);

            if (target is IPoolable poolable)
                poolable.OnReuse();

            return target;
        }

        private T Find(Transform parent)
        {
            T result;

            do {
                result = pool.Count != 0
                    ? pool.Dequeue()
                    : PoolOrchestrator.Find<T>(prefabName) ?? Create(parent);
            } while (result == null);

            return result;
        }

        public void Recycle(T target)
        {
            if (target == null)
                return;

            var go = target.gameObject;

            go.SetActive(false);
            PoolOrchestrator.Recycle(go);

            if (target is IPoolable poolable)
                poolable.OnRecycle();

            pool.Enqueue(target);
        }

        protected T Create(Transform parent)
        {
            prefab ??= PoolOrchestrator.GetPrefab(prefabName);
            var component = PoolOrchestrator.Create<T>(prefabName, prefab, parent);
            return component;
        }

        private void EnsureOrphansAreCollected()
        {
            if (hasCollectedOrphans)
                return;

            hasCollectedOrphans = true;

            foreach (var orphan in PoolOrchestrator.OrphanObjects<T>(prefabName)) {
                Recycle(orphan);
            }
        }
    }
}
