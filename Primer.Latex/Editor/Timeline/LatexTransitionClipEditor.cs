using System.Collections.Generic;
using LatexRenderer.Timeline;
using UnityEditor.Timeline;
using UnityEngine;

namespace UnityEditor.LatexRenderer.Timeline
{
    [CustomEditor(typeof(LatexTransitionClip))]
    public class LatexTransitionClipEditor : Editor
    {
        private TransitionListDrawer _morphTransitionsDrawer;
        private TransitionListDrawer _scaleDownAndMoveTransitionsDrawer;
        private LatexTransitionClip Clip => (LatexTransitionClip)target;

        private void OnEnable()
        {
            _morphTransitionsDrawer = new TransitionListDrawer(
                serializedObject.FindProperty(nameof(LatexTransitionClip.morphTransitions)));
            _scaleDownAndMoveTransitionsDrawer = new TransitionListDrawer(
                serializedObject.FindProperty(
                    nameof(LatexTransitionClip.scaleDownAndMoveTransitions)));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var validationErrors = Clip.GetValidationErrors(TimelineEditor.inspectedDirector);
            if (validationErrors.Count > 0)
            {
                List<string> parts = new();
                for (var i = 0; i < validationErrors.Count; ++i)
                    parts.Add($"({i + 1}) {validationErrors[i]}");

                EditorGUILayout.HelpBox($"Validation errors: {string.Join(", ", parts)}",
                    MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox("OK", MessageType.Info);
            }

            var beforeProperty = serializedObject.FindProperty("before");
            var newBefore = EditorGUILayout.ObjectField(
                new GUIContent(beforeProperty.displayName, beforeProperty.tooltip),
                beforeProperty.exposedReferenceValue, typeof(Transform), true);

            var afterProperty = serializedObject.FindProperty("after");
            var newAfter = EditorGUILayout.ObjectField(
                new GUIContent(afterProperty.displayName, afterProperty.tooltip),
                afterProperty.exposedReferenceValue, typeof(Transform), true);

            if (beforeProperty.exposedReferenceValue is null ||
                afterProperty.exposedReferenceValue is null)
            {
                EditorGUILayout.HelpBox(
                    "Before and After must be set before any other properties are changed.",
                    MessageType.Info);
            }
            else
            {
                var beforeAnchorProperty =
                    serializedObject.FindProperty(nameof(LatexTransitionClip.beforeAnchor));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent(beforeAnchorProperty.displayName,
                    beforeAnchorProperty.tooltip));
                ChildDropdown.DrawLayout(beforeAnchorProperty,
                    beforeProperty.exposedReferenceValue as Transform);
                EditorGUILayout.EndHorizontal();

                var afterAnchorProperty =
                    serializedObject.FindProperty(nameof(LatexTransitionClip.afterAnchor));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent(afterAnchorProperty.displayName,
                    afterAnchorProperty.tooltip));
                ChildDropdown.DrawLayout(afterAnchorProperty,
                    afterProperty.exposedReferenceValue as Transform);
                EditorGUILayout.EndHorizontal();

                _morphTransitionsDrawer.DrawLayout();
                _scaleDownAndMoveTransitionsDrawer.DrawLayout();
            }

            beforeProperty.SetExposedReference(newBefore);
            afterProperty.SetExposedReference(newAfter);
            serializedObject.ApplyModifiedProperties();
        }
    }
}