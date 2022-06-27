using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using LatexRenderer;

namespace UnityEditor.LatexRenderer
{
    [CustomEditor(typeof(LatexRendererComponent))]
    public class LatexRendererComponentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Release SVG Parts"))
            {
                Undo.SetCurrentGroupName("Release SVG Parts");
                
                var component = (LatexRendererComponent)target;

                foreach (var svgPart in component._svgParts)
                {
                    // TODO: This isn't working as expected... See the HACK in LatexRendererComponent.Start.
                    Undo.RecordObject(svgPart, "");
                    svgPart.hideFlags = HideFlags.None;
                }

                // If we didn't clear _svgParts, the component would destroy them when it was destroyed
                Undo.RecordObject(component, "");
                component._svgParts = new List<GameObject>();
                Undo.DestroyObjectImmediate(component);
            }
        }
    }
}