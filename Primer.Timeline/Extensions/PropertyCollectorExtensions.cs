using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

namespace Primer.Timeline
{
    public static class PropertyCollectorExtensions
    {
        /// <summary>Add a property of <paramref name="target" /> to the Registrar.</summary>
        /// <param name="target">The object that may be modified.</param>
        /// <param name="properties">
        ///     The paths of the <em>serialized properties</em> that may be modified,
        ///     ex: <c>"m_Enabled"</c>, <c>"m_SensorSize.x"</c>.
        ///     See the remarks section for details.
        /// </param>
        /// <remarks>
        ///     Serialized property names for Unity objects are often not the names of the members they
        ///     are accessed via. For example, <c>Camera.enabled</c> is how you would access the serialized
        ///     property with the name <c>"m_Enabled"</c>. Serialized property names are generally the names
        ///     of the private data members of the object.
        /// </remarks>
        /// <exception cref="ArgumentException">
        ///     If any of the paths in <paramref name="properties" /> are invalid, an exception will be thrown
        ///     with a detailed description listing all of the valid options.
        /// </exception>
        public static void AddProperties(this IPropertyCollector self, Component target, params string[] properties)
            => AddPropertiesDynamic(self, target, properties);

        /// <see cref="AddProperties(IPropertyCollector, UnityEngine.Component, string[])" />
        public static void AddProperties(this IPropertyCollector self, GameObject target, params string[] properties)
            => AddPropertiesDynamic(self, target, properties);


        private static void AddPropertiesDynamic(IPropertyCollector collector, UnityEngine.Object target, params string[] propertyPaths) {
#if UNITY_EDITOR
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var serializedObject = new SerializedObject(target);

            foreach (var propertyPath in propertyPaths) {
                AssertPropertyExistsIn(serializedObject, propertyPath);

                if (target is GameObject gameObject) {
                    collector.AddFromName(gameObject, propertyPath);
                    continue;
                }

                if (target is Component component) {
                    collector.AddFromName(component, propertyPath);
                    continue;
                }

                throw new ArgumentException(
                    $"Expected type of GameObject or Component, got {target.GetType().Name}",
                    nameof(target)
                );
            }
        }

        private static void AssertPropertyExistsIn(SerializedObject serializedObject, string propertyPath)
        {
            if (serializedObject.FindProperty(propertyPath) is not null)
                return;

            var typeName = serializedObject.targetObject.GetType().Name;
            var paths = ListPropertyPaths(serializedObject);
            var prettyOptions = string.Join(",\n", paths.Select(i => $"\t\"{i}\""));

            throw new ArgumentException(
                $"Serialized property \"{propertyPath}\" does not exist in {typeName}. Expected one of:\n{prettyOptions}",
                nameof(propertyPath)
            );
        }

        private static IEnumerable<string> ListPropertyPaths(SerializedObject serializedObject)
        {
            var paths = new List<string>();
            var property = serializedObject.GetIterator();

            while (property.Next(true))
                paths.Add(property.propertyPath);

            return paths;
#endif
        }
    }
}
