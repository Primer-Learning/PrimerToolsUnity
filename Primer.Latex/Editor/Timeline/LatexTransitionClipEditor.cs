using LatexRenderer.Timeline;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.LatexRenderer.Timeline
{
    [CustomEditor(typeof(LatexTransitionClip))]
    public class LatexTransitionClipEditor : Editor
    {
        private ReorderableList _morphsListElement;
        private LatexTransitionClip Clip => (LatexTransitionClip)target;

        private void OnEnable()
        {
            var morphDrawer = new MorphDrawer(Clip.before.Resolve(), Clip.after.Resolve());
            var morphTransitionsProperty =
                serializedObject.FindProperty(nameof(LatexTransitionClip.morphTransitions));
            _morphsListElement = new ReorderableList(serializedObject, morphTransitionsProperty)
            {
                displayAdd = true,
                displayRemove = true,
                draggable = true,
                drawHeaderCallback = rect =>
                    EditorGUI.LabelField(rect, morphTransitionsProperty.displayName),
                drawElementCallback = (rect, index, focused, active) =>
                {
                    var element = morphTransitionsProperty.GetArrayElementAtIndex(index);
                    morphDrawer.OnGUI(rect, element);
                },
                elementHeightCallback = index => morphDrawer.GetPropertyHeight(),
                onAddCallback = list =>
                {
                    var at = list.serializedProperty.arraySize;
                    list.serializedProperty.InsertArrayElementAtIndex(at);
                    var newElement = list.serializedProperty.GetArrayElementAtIndex(at);
                    morphDrawer.Reset(newElement);
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, nameof(LatexTransitionClip.morphTransitions),
                nameof(LatexTransitionClip.beforeAnchor), nameof(LatexTransitionClip.afterAnchor));

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

            _morphsListElement.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}