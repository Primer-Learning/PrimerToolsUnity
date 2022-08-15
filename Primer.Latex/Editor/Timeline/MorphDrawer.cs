using System;
using System.Collections.Generic;
using System.Linq;
using LatexRenderer.Timeline;
using UnityEngine;

namespace UnityEditor.LatexRenderer.Timeline
{
    /// <summary>(Unofficial) property drawer for MorphTransitions.</summary>
    /// <remarks>
    ///     It's unofficial because we don't actually register it as the drawer. Instead it's used
    ///     directly by the editor. This is necessary because the ReorderableList is quite manual and we
    ///     can't just give it a drawer.
    /// </remarks>
    public class MorphDrawer
    {
        /// <summary>Should have the same value as LatexTransitionClip.after.</summary>
        public readonly Transform after;

        /// <summary>Should have the same value as LatexTransitionClip.before.</summary>
        public readonly Transform before;

        public MorphDrawer(Transform before, Transform after)
        {
            this.before = before;
            this.after = after;
        }

        /// <summary>Provides a sub-rect of the full Rect we're to draw into.</summary>
        /// <param name="position">The full rect containing all subsections.</param>
        /// <param name="subsection">The subsection to calculate.</param>
        private Rect GetSubSection(Rect position, Subsection subsection)
        {
            switch (subsection)
            {
                case Subsection.TopLeft:
                    var topLeft = position;
                    topLeft.width /= 2;
                    topLeft.height /= 2;
                    return topLeft;

                case Subsection.BottomLeft:
                    var bottomLeft = position;
                    bottomLeft.width /= 2;
                    bottomLeft.height /= 2;
                    bottomLeft.y += bottomLeft.height;
                    return bottomLeft;

                case Subsection.TopRight:
                    var topRight = position;
                    topRight.width /= 2;
                    topRight.x += topRight.width;
                    topRight.height /= 2;
                    return topRight;

                case Subsection.BottomRight:
                    var bottomRight = position;
                    bottomRight.width /= 2;
                    bottomRight.x += bottomRight.width;
                    bottomRight.height /= 2;
                    bottomRight.y += bottomRight.height;
                    return bottomRight;

                default:
                    throw new ArgumentException($"Unrecognized subsection ${subsection}");
            }
        }

        /// <summary>Generate the dropdown options for a given parent.</summary>
        /// <param name="parent">Should be either after or before.</param>
        /// <param name="indexPrefix">
        ///     Used during recursion to keep track of the stack of indices. Don't
        ///     specify directly.
        /// </param>
        private List<Option> GetOptions(Transform parent, List<int> indexPrefix = null)
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

        /// <summary>Draws a dropdown menu for the given property.</summary>
        private void OptionsField(Rect position, SerializedProperty property, List<Option> options)
        {
            var child = property.exposedReferenceValue as Transform;
            var index = options.FindIndex(i => i.Transform == child);

            var newIndex = EditorGUI.Popup(position, index,
                options.Select(i => i.DisplayName).ToArray());
            if (newIndex != index)
                property.exposedReferenceValue = options[newIndex].Transform;
        }

        /// <summary>Draws property within the given rect.</summary>
        public void OnGUI(Rect position, SerializedProperty property)
        {
            EditorGUI.LabelField(GetSubSection(position, Subsection.TopLeft), "Before");
            OptionsField(GetSubSection(position, Subsection.BottomLeft),
                property.FindPropertyRelative(
                    nameof(LatexTransitionClip.MorphTransition.beforeChild)), GetOptions(before));

            EditorGUI.LabelField(GetSubSection(position, Subsection.TopRight), "After");
            OptionsField(GetSubSection(position, Subsection.BottomRight),
                property.FindPropertyRelative(
                    nameof(LatexTransitionClip.MorphTransition.afterChild)), GetOptions(after));
        }

        /// <summary>Resets a serialized MorphTransition to default values.</summary>
        public void Reset(SerializedProperty property)
        {
            foreach (var name in new[]
                     {
                         nameof(LatexTransitionClip.MorphTransition.beforeChild),
                         nameof(LatexTransitionClip.MorphTransition.afterChild)
                     })
            {
                var exposedReference = new ExposedReference<Transform>();
                exposedReference.Set(null);
                exposedReference.CopyToSerializedProperty(property.FindPropertyRelative(name));
            }
        }

        public float GetPropertyHeight()
        {
            return EditorGUIUtility.singleLineHeight * 2;
        }


        private struct Option
        {
            public Transform Transform;
            public List<int> Indices;

            public string DisplayName =>
                Transform ? $"{string.Join(".", Indices)}\t{Transform.gameObject.name}" : "";
        }


        private enum Subsection
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }
    }
}