using System.Reflection;
using UnityEngine;

namespace Primer
{
    public static class ComponentExtensions
    {
        public static void CopyPublicComponentValuesFrom<T>(this T destination, T source) where T : Component
        {
            // If either source or destination is null, do nothing
            if (source == null || destination == null)
            {
                Debug.LogWarning("Source or destination is null. Cannot copy component values.");
                return;
            }

            FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                field.SetValue(destination, field.GetValue(source));
            }

            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                // Skip properties that don't have a setter
                if (property.CanWrite)
                {
                    property.SetValue(destination, property.GetValue(source, null), null);
                }
            }
        }
    }
}