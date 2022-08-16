using System;
using UnityEditor.Timeline;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityEditor.LatexRenderer.Timeline
{
    public static class ExposedReferenceExtension
    {
        /// <summary>
        ///     Sets a SerializedProperty's <code>exposedReferenceValue</code> property. Always use this
        ///     instead of its setter.
        /// </summary>
        /// <param name="property">The property to modify.</param>
        /// <param name="value">The value to set <code>exposedReferenceValue<code> to.</param>
        /// <param name="regenerateExposedName">
        ///     An <code>exposedReference</code> is a pointer. If this is
        ///     `false` this function will change the value the pointer points to, otherwise it will allocate a
        ///     new value with the given value and set the pointer to point at that new value.
        /// </param>
        /// <remarks>
        ///     This has several differences from <code>exposedReferenceValue</code>'s setter: (1) we
        ///     always set <code>defaultValue</code> to null to prevent silent failures, (2) we raise an
        ///     exception if the the <code>SerializedProperty</code> is not attached to a scene with a playable
        ///     director, (3) we register the playable director with the undo system so Unity knows that it
        ///     changed, and (4) we give a means to regenerate the exposed name.
        /// </remarks>
        public static void SetExposedReference<T>(this SerializedProperty property, T value,
            bool regenerateExposedName = false) where T : Object
        {
            if (property.propertyType != SerializedPropertyType.ExposedReference)
                throw new InvalidOperationException(
                    "Attempting to set the reference value on a SerializedProperty that is not an ExposedReference.");
            if (property.serializedObject.context is not IExposedPropertyTable context)
                throw new InvalidOperationException(
                    "Attempting to set the reference value on a SerializedProperty that is not attached to a scene with a playable director.");

            var defaultValueProperty = property.FindPropertyRelative("defaultValue");
            defaultValueProperty.objectReferenceValue = null;

            var exposedNameProperty = property.FindPropertyRelative("exposedName");
            if (regenerateExposedName || string.IsNullOrEmpty(exposedNameProperty.stringValue))
                exposedNameProperty.stringValue = GUID.Generate().ToString();

            Undo.RecordObject(context as Object, $"Set {property.displayName}");
            context.SetReferenceValue(exposedNameProperty.stringValue, value);
        }

        /// <summary>Gets the value the exposed reference points to.</summary>
        /// <remarks>
        ///     This is only guaranteed to work within an editor for an object within the timeline window
        ///     (since the timeline window needs to be open and a timeline asset selected).
        /// </remarks>
        public static T Resolve<T>(this ref ExposedReference<T> exposedReference) where T : Object
        {
            return exposedReference.Resolve(TimelineEditor.inspectedDirector);
        }
    }
}