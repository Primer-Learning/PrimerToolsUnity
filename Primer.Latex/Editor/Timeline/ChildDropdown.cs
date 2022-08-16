using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.LatexRenderer.Timeline
{
    public class ChildDropdown
    {
        /// <summary>Generate the dropdown options for a given parent.</summary>
        /// <param name="parent">Should be either after or before.</param>
        /// <param name="indexPrefix">
        ///     Used during recursion to keep track of the stack of indices. Don't
        ///     specify directly.
        /// </param>
        private static List<Option> GetOptions(Transform parent, List<int> indexPrefix = null)
        {
            List<Option> options = new();
            options.Add(new Option { Transform = null, Indices = new List<int>() });

            // Within the foreach loop, index of child in list of parent's children
            var index = 0;

            foreach (Transform child in parent)
            {
                var indices = indexPrefix is null ? new List<int>() : new List<int>(indexPrefix);
                indices.Add(index);

                options.Add(new Option { Transform = child, Indices = indices });

                options.AddRange(GetOptions(child, indices));

                ++index;
            }

            return options;
        }

        public static void Draw(Rect position, SerializedProperty property, Transform parent)
        {
            var options = GetOptions(parent);
            var child = property.exposedReferenceValue as Transform;
            var index = options.FindIndex(i => i.Transform == child);

            var newIndex = EditorGUI.Popup(position, index,
                options.Select(i => i.DisplayName).ToArray());
            if (newIndex != index)
                property.SetExposedReference(options[newIndex].Transform);
        }

        public static void DrawLayout(SerializedProperty property, Transform parent)
        {
            var options = GetOptions(parent);
            var child = property.exposedReferenceValue as Transform;
            var index = options.FindIndex(i => i.Transform == child);

            var newIndex =
                EditorGUILayout.Popup(index, options.Select(i => i.DisplayName).ToArray());
            if (newIndex != index)
                property.SetExposedReference(options[newIndex].Transform);
        }

        public struct Option
        {
            public Transform Transform;
            public List<int> Indices;

            public string DisplayName =>
                Transform ? $"{string.Join(".", Indices)}\t{Transform.gameObject.name}" : "";
        }
    }
}