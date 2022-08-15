using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;

namespace UnityEditor.LatexRenderer.Timeline
{
    public static class ExposedReferenceExtension
    {
        /// <summary>Sets the value of an exposed reference.</summary>
        /// <param name="exposedReference"></param>
        /// <param name="playableDirector">
        ///     The playable director for the scene. The exposed reference will only
        ///     be resolvable in that scene. If you're in the inspector for a Timeline, you can use
        ///     <code>TimelineEditor.inspectedDirector</code> to get the director associated with the open
        ///     Timeline window.
        /// </param>
        /// <param name="value">The object to reference.</param>
        /// <param name="defaultValue">
        ///     If resolution fails, the exposed reference will silently resolve to this
        ///     value.
        /// </param>
        /// <typeparam name="T"></typeparam>
        public static void Set<T>(this ref ExposedReference<T> exposedReference,
            PlayableDirector playableDirector, T value, T defaultValue = null) where T : Object
        {
            exposedReference.exposedName = GUID.Generate().ToString();
            exposedReference.defaultValue = defaultValue;
            playableDirector.SetReferenceValue(exposedReference.exposedName, value);
        }

        /// <summary>Sets the value of an exposed reference.</summary>
        /// <param name="exposedReference"></param>
        /// <param name="value">The object to reference.</param>
        /// <param name="defaultValue">
        ///     If resolution fails, the exposed reference will silently resolve to this
        ///     value.
        /// </param>
        /// <typeparam name="T"></typeparam>
        public static void Set<T>(this ref ExposedReference<T> exposedReference, T value,
            T defaultValue = null) where T : Object
        {
            exposedReference.Set(TimelineEditor.inspectedDirector, value, defaultValue);
        }

        public static void CopyToSerializedProperty<T>(
            this ref ExposedReference<T> exposedReference, SerializedProperty property)
            where T : Object
        {
            // Note that PropertyName.ToString() always gives "UnityEngine.PropertyName" in the
            // player. Fortunately we're in editor-only land so it actually gives the real string.
            property.FindPropertyRelative(nameof(ExposedReference<Transform>.exposedName))
                .stringValue = exposedReference.exposedName.ToString();

            property.FindPropertyRelative(nameof(ExposedReference<Transform>.defaultValue))
                .objectReferenceValue = null;
        }

        public static T Resolve<T>(this ref ExposedReference<T> exposedReference) where T : Object
        {
            return exposedReference.Resolve(TimelineEditor.inspectedDirector);
        }
    }
}