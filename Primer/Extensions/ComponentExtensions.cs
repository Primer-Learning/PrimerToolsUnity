using System;
using UnityEngine;

namespace Primer
{
    public static class ComponentExtensions
    {
        public static T GetOrAddComponent<T>(this Component component) where T : Component =>
            component.GetComponent<T>() ?? component.gameObject.AddComponent<T>();
    }
}
