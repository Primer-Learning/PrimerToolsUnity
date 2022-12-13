using Primer.Editor;
using UnityEditor;

namespace Primer.Scene.Editor
{
    [CustomEditor(typeof(RenderToPng))]
    public class RenderToPngEditor : PrimerEditor<RenderToPng>
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            using (new EditorGUI.DisabledScope(true)) {
                EditorGUILayout.IntField("Frames saved", component.framesSaved);
                EditorGUILayout.TextField("Default out dir", RenderToPng.defaultOutDir);
            }
        }
    }
}
