using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LatexRendererComponent))]
public class LatexRendererComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Release SVG Parts"))
        {
            var component = (LatexRendererComponent)target;

            foreach (var svgPart in component._svgParts)
            {
                Undo.RecordObject(svgPart, "Enable");
                svgPart.hideFlags = HideFlags.None;
            }

            // If we didn't clear _svgParts, the component would destroy them when it was destroyed
            Undo.RecordObject(component, "");
            component._svgParts = new List<GameObject>();
            Undo.DestroyObjectImmediate(component);
        }
    }
}