using System;
using UnityEditor.Timeline;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityEditor.LatexRenderer.Timeline
{
    public static class ExposedReferenceExtension
    {
        /// <summary>Sets a SerializedProperty's `exposedReferenceValue` property.</summary>
        /// <param name="property">The property to modify.</param>
        /// <param name="value">The value to set `exposedReferenceValue` to.</param>
        /// <param name="regenerateExposedName">
        ///     Generates a new `exposedName` regardless of its current value
        ///     (a new one will always be generated if the current `exposedName` is null). `exposedReferences`
        ///     with the same `exposedName` all point to the same object, so regenerating the `exposedName`
        ///     allows this reference to point to an object independent of any others.
        /// </param>
        /// <remarks>
        ///     The setter of `exposedReferenceValue` will silently fail if SerializedProperty is not
        ///     attached to a scene properly. This function will instead throw an exception. This function also
        ///     always sets the `defaultValue`, rather than only doing so in that silent failure case.
        /// </remarks>
        public static void SetExposedReference<T>(this SerializedProperty property, T value,
            bool regenerateExposedName = false) where T : Object
        {
            if (property.propertyType != SerializedPropertyType.ExposedReference)
                throw new InvalidOperationException(
                    "Attempting to set the reference value on a SerializedProperty that is not an ExposedReference.");
            if (property.serializedObject.context is not IExposedPropertyTable context)
                throw new InvalidOperationException(
                    "Attempting to set the reference value on a SerializedProperty that is not attached to a scene.");

            var defaultValueProperty = property.FindPropertyRelative("defaultValue");
            var exposedNameProperty = property.FindPropertyRelative("exposedName");
            defaultValueProperty.objectReferenceValue = value;
            if (regenerateExposedName || string.IsNullOrEmpty(exposedNameProperty.stringValue))
                exposedNameProperty.stringValue = GUID.Generate().ToString();

            context.SetReferenceValue(exposedNameProperty.stringValue, value);
        }

        public static T Resolve<T>(this ref ExposedReference<T> exposedReference) where T : Object
        {
            return exposedReference.Resolve(TimelineEditor.inspectedDirector);
        }
    }
}