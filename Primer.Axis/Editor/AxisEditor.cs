using Primer.Axis;
using Primer.Editor;
using UnityEditor;

namespace Primer.Graph.Editor
{
    [CustomEditor(typeof(Axis.AxisRenderer))]
    public class AxisEditor : PrimerEditor<Axis.AxisRenderer>
    {
        public override void OnInspectorGUI() {
            EditorGUI.BeginChangeCheck();

            CustomHeader(component.gameObject.name);
            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck()) {
                component.UpdateChildren();
            }
        }
    }
}
