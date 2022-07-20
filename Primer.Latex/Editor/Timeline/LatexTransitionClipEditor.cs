using System.Collections.Generic;
using System.Linq;
using LatexRenderer.Timeline;
using UnityEditor.Timeline;
using UnityEngine;

namespace UnityEditor.LatexRenderer.Timeline
{
    [CustomEditor(typeof(LatexTransitionClip))]
    public class LatexTransitionClipEditor : Editor
    {
        private LatexTransitionClip Clip => (LatexTransitionClip)target;

        private ExposedReference<Transform> CreateReference(Transform transform)
        {
            ExposedReference<Transform> result = new();
            result.Set(TimelineEditor.inspectedDirector, transform);
            return result;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "morphTransitions");
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.LabelField("Morph Transitions");

            var toDelete = new List<int>();

            var playableDirector = TimelineEditor.inspectedDirector;
            var morphs = Clip.morphTransitions;
            for (var i = 0; i < morphs.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Before Child");
                var oldBeforeChild = morphs[i].beforeChild.Resolve(playableDirector);
                var newBeforeChild =
                    EditorGUILayout.ObjectField(oldBeforeChild, typeof(Transform), true) as
                        Transform;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("After Child");
                var oldAfterChild = morphs[i].afterChild.Resolve(playableDirector);
                var newAfterChild =
                    EditorGUILayout.ObjectField(oldAfterChild, typeof(Transform),
                        true) as Transform;
                EditorGUILayout.EndHorizontal();

                if (oldBeforeChild != newBeforeChild || oldAfterChild != newAfterChild)
                    morphs[i] = new LatexTransitionClip.MorphTransition
                    {
                        beforeChild = CreateReference(newBeforeChild),
                        afterChild = CreateReference(newAfterChild)
                    };

                if (GUILayout.Button("Delete")) toDelete.Add(i);
            }

            foreach (var i in toDelete.AsEnumerable().Reverse()) morphs.RemoveAt(i);

            if (GUILayout.Button("Add New")) morphs.Add(new LatexTransitionClip.MorphTransition());

            serializedObject.Update();

            // serializedObject.Update();
            //
            // // var newValue =
            // //     (Transform)EditorGUILayout.ObjectField(Clip.single.Resolve(playableDirector),
            // //         typeof(Transform), true);
            // //
            // // ExposedReference<Transform> foo = new();
            // // foo.Set(playableDirector, newValue);
            //
            // // Clip.single = foo;
            //
            //
            // if (GUILayout.Button("Add Target")) targets.Add(new ExposedReference<Transform>());
            //
            //
            // serializedObject.Update();
        }
    }
}