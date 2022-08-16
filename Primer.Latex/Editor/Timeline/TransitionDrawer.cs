using System;
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
    public class TransitionDrawer
    {
        /// <summary>Should have the same value as LatexTransitionClip.after.</summary>
        public readonly Transform after;

        /// <summary>Should have the same value as LatexTransitionClip.before.</summary>
        public readonly Transform before;

        public TransitionDrawer(Transform before, Transform after)
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

        /// <summary>Draws property within the given rect.</summary>
        public void OnGUI(Rect position, SerializedProperty property)
        {
            EditorGUI.LabelField(GetSubSection(position, Subsection.TopLeft), "Before");
            ChildDropdown.Draw(GetSubSection(position, Subsection.BottomLeft),
                property.FindPropertyRelative(nameof(LatexTransitionClip.Transition.beforeChild)),
                before);

            EditorGUI.LabelField(GetSubSection(position, Subsection.TopRight), "After");
            ChildDropdown.Draw(GetSubSection(position, Subsection.BottomRight),
                property.FindPropertyRelative(nameof(LatexTransitionClip.Transition.afterChild)),
                after);
        }

        /// <summary>Resets a serialized Transition to default values.</summary>
        public void Reset(SerializedProperty property)
        {
            foreach (var name in new[]
                     {
                         nameof(LatexTransitionClip.Transition.beforeChild),
                         nameof(LatexTransitionClip.Transition.afterChild)
                     })
                property.FindPropertyRelative(name).SetExposedReference<Transform>(null, true);
        }

        public float GetPropertyHeight()
        {
            return EditorGUIUtility.singleLineHeight * 2;
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