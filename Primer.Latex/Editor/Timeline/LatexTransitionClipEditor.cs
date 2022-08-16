using LatexRenderer.Timeline;
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
            _morphTransitionsDrawer = new TransitionListDrawer(Clip.before.Resolve(),
                Clip.after.Resolve(),
                serializedObject.FindProperty(nameof(LatexTransitionClip.morphTransitions)));
            _scaleDownAndMoveTransitionsDrawer = new TransitionListDrawer(Clip.before.Resolve(),
                Clip.after.Resolve(),
                serializedObject.FindProperty(
                    nameof(LatexTransitionClip.scaleDownAndMoveTransitions)));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, nameof(LatexTransitionClip.morphTransitions),
                nameof(LatexTransitionClip.beforeAnchor), nameof(LatexTransitionClip.afterAnchor),
                nameof(LatexTransitionClip.scaleDownAndMoveTransitions));

            var beforeAnchorProperty =
                serializedObject.FindProperty(nameof(LatexTransitionClip.beforeAnchor));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent(beforeAnchorProperty.displayName,
                beforeAnchorProperty.tooltip));
            ChildDropdown.DrawLayout(beforeAnchorProperty, Clip.before.Resolve());
            EditorGUILayout.EndHorizontal();

            var afterAnchorProperty =
                serializedObject.FindProperty(nameof(LatexTransitionClip.afterAnchor));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent(afterAnchorProperty.displayName,
                afterAnchorProperty.tooltip));
            ChildDropdown.DrawLayout(afterAnchorProperty, Clip.after.Resolve());
            EditorGUILayout.EndHorizontal();

            _morphTransitionsDrawer.DrawLayout();
            _scaleDownAndMoveTransitionsDrawer.DrawLayout();

            serializedObject.ApplyModifiedProperties();
        }
    }
}