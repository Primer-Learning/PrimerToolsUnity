using UnityEngine;

namespace Primer
{
    public interface IPool<T> where T : Component
    {
        public T Get(Transform parent = null);

        public void Recycle(T target);

        public void RecycleAll();

        public void Fill(int amount);
    }
}
