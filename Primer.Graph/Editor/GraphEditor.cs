using Primer.Editor;
using UnityEditor;

namespace Primer.Graph.Editor
{
    [CustomEditor(typeof(Graph2))]
    public class GraphEditor : PrimerEditor<Graph2>
    {
        public override void OnInspectorGUI() {
            EditorGUI.BeginChangeCheck();

            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck()) {
                component.Regenerate();
            }
        }
    }
}
