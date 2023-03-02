using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Primer.Tools.Editor
{
    [CustomEditor(typeof(PrimerBracket2)), CanEditMultipleObjects]
    public class PrimerBracketEditor : OdinEditor
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
            var bracket = (PrimerBracket2)target;
            var parent = bracket.transform.parent;

            EditorGUI.BeginChangeCheck();

            bracket.anchorPoint.DrawHandle(parent);
            bracket.leftPoint.DrawHandle(parent);
            bracket.rightPoint.DrawHandle(parent);

            if (!EditorGUI.EndChangeCheck())
                return;

            Undo.RecordObject(bracket, "Change start / end");
            bracket.Refresh();
        }
    }
}
