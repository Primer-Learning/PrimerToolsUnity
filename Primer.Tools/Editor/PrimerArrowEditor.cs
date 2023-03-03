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
            var parent = arrow.transform.parent;

            EditorGUI.BeginChangeCheck();

            arrow.startPoint.DrawHandle(parent);
            arrow.endPoint.DrawHandle(parent);

            if (!EditorGUI.EndChangeCheck())
                return;

            Undo.RecordObject(arrow, "Change start / end");
            arrow.Recalculate();
        }
    }
}
