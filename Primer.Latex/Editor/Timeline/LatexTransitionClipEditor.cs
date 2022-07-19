using LatexRenderer.Timeline;

namespace UnityEditor.LatexRenderer.Timeline
{
    [CustomEditor(typeof(LatexTransitionClip))]
    public class LatexTransitionClipEditor : Editor
    {
        private LatexTransitionClip Clip => (LatexTransitionClip)target;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            // serializedObject.Update();
            // var playableDirector = TimelineEditor.inspectedDirector;
            // var targets = Clip.targets;
            // for (var i = 0; i < targets.Count; ++i)
            // {
            //     var newValue = EditorGUILayout.ObjectField(targets[i].Resolve(playableDirector),
            //         typeof(Transform), true) as Transform;
            //
            //     ExposedReference<Transform> foo = new();
            //     foo.Set(playableDirector, newValue);
            //     targets[i] = foo;
            // }
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