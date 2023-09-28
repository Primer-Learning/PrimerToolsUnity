using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Primer.Shapes.Editor
{
    [CustomEditor(typeof(PrimerArrow3))]
    public class PrimerArrow3Editor : OdinEditor
    {
        protected void OnSceneGUI()
        {
            PrimerArrow3 arrow = (PrimerArrow3)target;
        
            EditorGUI.BeginChangeCheck();
            
            Vector3 newTailPosition = Handles.PositionHandle(arrow.tailObject.position, Quaternion.identity);
            
            if (EditorGUI.EndChangeCheck())
            {
                if (arrow.transformThatTailFollows != null)
                {
                    Debug.LogWarning("Can't use the handle when the tail is following another transform.");
                }
                
                Undo.RecordObject(arrow, "Change Look At Target Position");
                if (arrow.transform.parent == null)
                    arrow._tailPoint = newTailPosition - arrow.transform.position;
                else
                {
                    arrow._tailPoint = arrow.transform.parent.InverseTransformPoint(newTailPosition) - arrow.transform.localPosition;
                }
                arrow.Update();
            }
        }
    }
}