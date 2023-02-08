using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Primer.Tools.Editor
{
    [CustomEditor(typeof(PrimerArrow2)), CanEditMultipleObjects]
    public class PrimerArrowEditor : OdinEditor
    {
        protected override void OnEnable()
        {
            UnityEditor.Tools.hidden = true;
        }

        protected override void OnDisable()
        {
            UnityEditor.Tools.hidden = false;
        }

        protected virtual void OnSceneGUI()
        {
            var arrow = (PrimerArrow2)target;
            var modifier = arrow.globalPositioning
                ? Vector3.zero
                : arrow.transform.parent?.position ?? Vector3.zero;

            EditorGUI.BeginChangeCheck();

            var start = Handles.PositionHandle(arrow.start + modifier, Quaternion.identity);
            var end = Handles.PositionHandle(arrow.end + modifier, Quaternion.identity);

            if (!EditorGUI.EndChangeCheck())
                return;

            Undo.RecordObject(arrow, "Change start / end");
            arrow.start = start - modifier;
            arrow.end = end - modifier;
            arrow.OnValidate();
        }
    }
}
