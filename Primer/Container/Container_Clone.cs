using UnityEngine;

namespace Primer
{
    public partial class Container<TComponent>
    {
        public static Container<T> Clone<T>(Container<T> template, string name) where T : Component
        {
            var rootObject = GetRootCloneOf(template.component, name);
            return new Container<T>(rootObject);
        }

        public static T Clone<T>(T template, string name) where T : Component
        {
            var rootObject = GetRootCloneOf(template, name);
            rootObject.SetActive(true);
            return rootObject;
        }
    }
}
