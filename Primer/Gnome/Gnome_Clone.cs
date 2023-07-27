using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Primer
{
    public partial class Gnome<TComponent>
    {
        public static Gnome<T> Clone<T>(Gnome<T> template, string name) where T : Component
        {
            var rootObject = GetRootCloneOf(template.component, name);
            return new Gnome<T>(rootObject);
        }

        public static T Clone<T>(T template, string name) where T : Component
        {
            var rootObject = GetRootCloneOf(template, name);
            rootObject.SetActive(true);
            return rootObject;
        }

        private static T GetRootCloneOf<T>(T template, string name) where T : Component
        {
            var scene = SceneManager.GetActiveScene();
            var rootGameObjects = scene.GetRootGameObjects();

            var found = rootGameObjects
                .FirstOrDefault(x => x.name == name && InstantiationTracker.IsInstanceOf(x, template))
                ?.GetComponent<T>();

            return found != null ? found : InstantiationTracker.InstantiateAndRegister(template, name);
        }
    }
}
