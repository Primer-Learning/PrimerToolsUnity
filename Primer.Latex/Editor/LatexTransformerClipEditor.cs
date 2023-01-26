using Primer.Animation;
using Primer.Editor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;

namespace Primer.Latex.Editor
{
    [CustomEditor(typeof(LatexTransformerClip))]
    public class LatexTransformerClipEditor : PrimerEditor<LatexTransformerClip>
    {
        private static readonly GUILayoutOption popupSize = GUILayout.MaxWidth(100);

        public override bool RequiresConstantRepaint() => true;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var director = TimelineEditor.inspectedDirector;
            var fromRenderer = director.GetGenericBindingForClip(component) as LatexRenderer;
            var toRenderer = component.transformTo.Resolve(director);

            if (fromRenderer is null || toRenderer is null)
                return;

            var fromGroups = fromRenderer.ranges;
            var toGroups = toRenderer.ranges;
            var trans = component.transitions;
            var toIndex = 0;

            if (fromGroups is null || toGroups is null)
                return;

            LatexCharEditor.CharPreviewSize();

            for (var i = trans.Count; i < fromGroups.Count; i++)
                trans.Add(0);

            for (var i = 0; i < fromGroups.Count; i++) {
                Space();

                using (new GUILayout.HorizontalScope()) {
                    GUILayout.Label($"Track's group {i}");
                    trans[i] = (TransitionType)EditorGUILayout.EnumPopup(trans[i], popupSize);
                }

                LatexCharEditor.ShowGroup(fromRenderer.expression, fromGroups[i]);

                if (trans[i] == TransitionType.Remove || toIndex == toGroups.Count)
                    continue;

                LatexCharEditor.ShowGroup(toRenderer.expression, toGroups[toIndex]);
                toIndex++;
            }

            if (toIndex == toGroups.Count)
                return;

            Space();
            GUILayout.Label("Following groups won't be used");

            for (var i = toIndex; i < toGroups.Count; i++) {
                GUILayout.Label($"Clip's group {i}");
                LatexCharEditor.ShowGroup(toRenderer.expression, toGroups[i]);
            }
        }
    }
}
