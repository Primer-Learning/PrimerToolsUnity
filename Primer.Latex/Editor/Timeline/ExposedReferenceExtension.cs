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
    }
}