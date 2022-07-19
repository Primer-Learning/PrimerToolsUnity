using LatexRenderer.Timeline;
using UnityEditor.Timeline;
using UnityEngine;

namespace UnityEditor.LatexRenderer.Timeline
{
    [CustomEditor(typeof(ScaleDownClip))]
    public class ScaleDownClipEditor : Editor
    {
        private ScaleDownClip Clip => (ScaleDownClip)target;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();


            var playableDirector = TimelineEditor.inspectedDirector;
            var targets = Clip.targets;
            for (var i = 0; i < targets.Count; ++i)
            {
                var newValue = EditorGUILayout.ObjectField(targets[i].Resolve(playableDirector),
                    typeof(Transform), true) as Transform;

                ExposedReference<Transform> foo = new();
                foo.Set(playableDirector, newValue);
                targets[i] = foo;
            }

            // var newValue =
            //     (Transform)EditorGUILayout.ObjectField(Clip.single.Resolve(playableDirector),
            //         typeof(Transform), true);
            //
            // ExposedReference<Transform> foo = new();
            // foo.Set(playableDirector, newValue);

            // Clip.single = foo;


            if (GUILayout.Button("Add Target")) targets.Add(new ExposedReference<Transform>());


            serializedObject.Update();
        }
    }
}