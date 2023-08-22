using System;
using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    // This is defined as extension methods because they doesn't need access to the internals of Gnome
    // But can become another _Partial class
    public static class Gnome_AddPrimitiveExtensions
    {
        private static readonly Dictionary<PrimitiveType, GameObject> primitives = new();

        public static Transform AddPrimitive(this Gnome self, PrimitiveType type, string name = null, ChildOptions options = null)
        {
            var primitive = GetPrimitive(type);
            return self.Add(primitive.transform, name ?? Enum.GetName(typeof(PrimitiveType), type), options);
        }

        public static T AddPrimitive<T>(this Gnome self, PrimitiveType type, string name = null, ChildOptions options = null)
            where T : Component
        {
            var primitive = GetPrimitive(type);
            var transform = self.Add(primitive.transform, name ?? Enum.GetName(typeof(PrimitiveType), type), options);
            return transform.GetOrAddComponent<T>();
        }

        private static GameObject GetPrimitive(PrimitiveType type)
        {
            if (primitives.TryGetValue(type, out var primitive)) {
                if (primitive == null)
                    primitives.Remove(type);
                else
                    return primitive;
            }

            primitive = GameObject.CreatePrimitive(type);
            primitive.hideFlags = HideFlags.HideInHierarchy;
            primitive.gameObject.SetActive(false);
            primitives.Add(type, primitive);
            return primitive;
        }
    }
}
