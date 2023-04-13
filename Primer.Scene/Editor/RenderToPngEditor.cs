using UnityEditor;

namespace Primer.Scene.Editor
{
    [CustomEditor(typeof(RenderToPng))]
    public class RenderToPngEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var component = (RenderToPng)target;

            using (new EditorGUI.DisabledScope(true)) {
                EditorGUILayout.IntField("Frames saved", component.framesSaved);
                EditorGUILayout.TextField("Default out dir", RenderToPng.defaultOutDir);
            }
        }
    }
}
