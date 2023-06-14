using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Primer
{
    public partial class Container<TComponent>
    {
        public Transform transform { get; }
        public TComponent component { get; }

        public Container(string name, Component parent = null)
        {
            transform = parent is null
                ? GetRootTransform(name)
                : GetDirectChild(parent.transform, name);

            component = GetComponent<TComponent>(transform);
            transform.SetActive(true);
            Reset();
        }

        public Container(TComponent component)
        {
            this.component = component;
            transform = component.transform;
            transform.SetActive(true);
            Reset();
        }

        public void Reset()
        {
            usedChildren.Clear();
            unusedChildren.Clear();
            unusedChildren.AddRange(ReadExistingChildren(transform));
        }

        private static Transform GetRootTransform(string name)
        {
            var scene = SceneManager.GetActiveScene();
            var rootGameObjects = scene.GetRootGameObjects();
            var found = rootGameObjects.FirstOrDefault(x => x.name == name);
            var obj = found != null ? found : new GameObject(name);
            return obj.transform;
        }

        private static Transform GetDirectChild(Transform parent, string name)
        {
            var found = parent.Find(name);

            if (found)
                return found;

            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            return child;
        }

        private static TResult GetComponent<TResult>(Component source) where TResult : Component
        {
            return typeof(TResult) == typeof(Transform)
                ? source as TResult
                : source.GetComponent<TResult>()
                ?? source.gameObject.AddComponent<TResult>();
        }
    }
}
