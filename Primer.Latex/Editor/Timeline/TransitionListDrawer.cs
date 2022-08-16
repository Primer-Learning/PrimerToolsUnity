using System;
using LatexRenderer.Timeline;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.LatexRenderer.Timeline
{
    /// <summary>(Unofficial) property drawer for MorphTransitions.</summary>
    /// <remarks>
    ///     It's unofficial because we don't actually register it as the drawer. Instead it's used
    ///     directly by the editor. This is necessary because the ReorderableList is quite manual and we
    ///     can't just give it a drawer.
    /// </remarks>
    public class TransitionListDrawer
    {
        /// <summary>Should have the same value as LatexTransitionClip._after.</summary>
        private readonly Transform _after;

        /// <summary>Should have the same value as LatexTransitionClip._before.</summary>
        private readonly Transform _before;

        private readonly ReorderableList _reorderableList;

        public TransitionListDrawer(Transform before, Transform after, SerializedProperty property)
        {
            _before = before;
            _after = after;

            _reorderableList = new ReorderableList(property.serializedObject, property)
            {
                displayAdd = true,
                displayRemove = true,
                draggable = true,
                drawHeaderCallback = rect =>
                    EditorGUI.LabelField(rect,
                        new GUIContent(property.displayName, property.tooltip)),
                drawElementCallback = (rect, index, focused, active) =>
                {
                    var element = property.GetArrayElementAtIndex(index);
                    OnGUI(rect, element);
                },
                elementHeightCallback = index => GetPropertyHeight(),
                onAddCallback = list =>
                {
                    // Create a new element in the list. It will be a duplicate of the last element
                    // in the list.
                    var at = list.serializedProperty.arraySize;
                    list.serializedProperty.InsertArrayElementAtIndex(at);
                    var newElement = list.serializedProperty.GetArrayElementAtIndex(at);

                    // The exposed references in the new element will point to the same value, so
                    // we want to allocate new null values for them.
                    foreach (var name in new[]
                             {
                                 nameof(LatexTransitionClip.Transition.beforeChild),
                                 nameof(LatexTransitionClip.Transition.afterChild)
                             })
                        newElement.FindPropertyRelative(name)
                            .SetExposedReference<Transform>(null, true);
                }
            };
        }

        /// <summary>Draws list using auto-layout (ie: EditorGUILayout).</summary>
        public void DrawLayout()
        {
            _reorderableList.DoLayoutList();
        }

        /// <summary>Provides a sub-rect of the full Rect we're to draw into.</summary>
        /// <param name="position">The full rect containing all subsections.</param>
        /// <param name="subsection">The subsection to calculate.</param>
        private static Rect GetSubSection(Rect position, Subsection subsection)
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
        private void OnGUI(Rect position, SerializedProperty property)
        {
            EditorGUI.LabelField(GetSubSection(position, Subsection.TopLeft), "Before");
            ChildDropdown.Draw(GetSubSection(position, Subsection.BottomLeft),
                property.FindPropertyRelative(nameof(LatexTransitionClip.Transition.beforeChild)),
                _before);

            EditorGUI.LabelField(GetSubSection(position, Subsection.TopRight), "After");
            ChildDropdown.Draw(GetSubSection(position, Subsection.BottomRight),
                property.FindPropertyRelative(nameof(LatexTransitionClip.Transition.afterChild)),
                _after);
        }

        private float GetPropertyHeight()
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