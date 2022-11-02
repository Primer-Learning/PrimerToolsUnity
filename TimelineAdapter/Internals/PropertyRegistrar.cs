using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace Primer.Timeline
{
    public class PropertyRegistrar
    {
        /// <summary>The underlying property collector we're wrapping.</summary>
        /// <remarks>
        ///     <tt>IPropertyCollector</tt>'s interface is difficult to work with directly so
        ///     <tt>PropertyRegistrar</tt> provides convenience methods on top of it. But the
        ///     <tt>IPropertyCollector</tt> can be used directly through this member if needed.
        /// </remarks>
        public readonly IPropertyCollector collector;

        public PropertyRegistrar(IPropertyCollector collector) => this.collector = collector;

#if UNITY_EDITOR
        void AssertPropertyExistsIn(SerializedObject serializedObject, string propertyPath) {
            if (serializedObject.FindProperty(propertyPath) is not null) return;

            List<string> allPropertyPaths = new();
            for (var property = serializedObject.GetIterator(); property.Next(true);)
                allPropertyPaths.Add(property.propertyPath);

            var typeName = serializedObject.targetObject.GetType().Name;
            var prettyOptions = string.Join(", ", allPropertyPaths.Select(i => $"\"{i}\""));
            throw new ArgumentException(
                $"Serialized property \"{propertyPath}\" does not exist in {typeName}. Expected one of: {prettyOptions}",
                "propertyPaths");
        }
#endif

        /// <summary>Add a property of <paramref name="target" /> to the Registrar.</summary>
        /// <param name="target">The object that may be modified.</param>
        /// <param name="propertyPaths">
        ///     The paths of the <em>serialized properties</em> that may be modified,
        ///     ex: <tt>"m_Enabled"</tt>, <tt>"m_SensorSize.x"</tt>. See the remarks section for details.
        /// </param>
        /// <remarks>
        ///     Serialized property names for Unity objects are often not the names of the members they
        ///     are accessed via. For example, <tt>Camera.enabled</tt> is how you would access the serialized
        ///     property with the name <tt>"m_Enabled"</tt>. Serialized property names are generally the names
        ///     of the private data members of the object.
        /// </remarks>
        /// <exception cref="ArgumentException">
        ///     If any of the paths in <paramref name="propertyPaths" /> are
        ///     invalid, an exception will be thrown with a detailed description listing all of the valid
        ///     options.
        /// </exception>
        public void AddProperties(Component target, params string[] propertyPaths) {
            AddPropertiesDynamic(target, propertyPaths);
        }

        /// <see cref="AddProperties(UnityEngine.Component,string[])" />
        public void AddProperties(GameObject target, params string[] propertyPaths) {
            AddPropertiesDynamic(target, propertyPaths);
        }

        void AddPropertiesDynamic(Object target, params string[] propertyPaths) {
#if UNITY_EDITOR
            if (target is null) throw new ArgumentNullException(nameof(target));

            var serializedObject = new SerializedObject(target);
            foreach (var propertyPath in propertyPaths) {
                AssertPropertyExistsIn(serializedObject, propertyPath);

                if (target is GameObject gameObject)
                    collector.AddFromName(gameObject, propertyPath);
                else if (target is Component component)
                    collector.AddFromName(component, propertyPath);
                else
                    throw new ArgumentException(
                        $"Expected type of GameObject or Component, got {target.GetType().Name}",
                        nameof(target));
            }
#endif
        }
    }
}
