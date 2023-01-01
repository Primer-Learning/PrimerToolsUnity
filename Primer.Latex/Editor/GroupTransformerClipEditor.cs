using Primer.Animation;
using Primer.Editor;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;

namespace Primer.Latex.Editor
{
    [CustomEditor(typeof(GroupTransformerClip))]
    public class GroupTransformerClipEditor : PrimerEditor<GroupTransformerClip>
    {
        private static readonly GUILayoutOption popupSize = GUILayout.MaxWidth(100);

        public override void OnInspectorGUI()
        {
            PropertyField(nameof(component.transformTo));

            var director = TimelineEditor.inspectedDirector;
            var fromRenderer = director.GetGenericBindingForClip(component) as LatexRenderer;
            var toRenderer = component.transformTo.Resolve(director);

            if (fromRenderer is null || toRenderer is null)
                return;

            var fromGroups = fromRenderer.ranges;
            var toGroups = toRenderer.ranges;
            var trans = component.transitions;
            var toIndex = 0;

            for (var i = trans.Count; i < fromGroups.Count; i++)
                trans.Add(0);

            for (var i = 0; i < fromGroups.Count; i++) {
                Space();

                using (new GUILayout.HorizontalScope()) {
                    GUILayout.Label($"Track's group {i}");
                    trans[i] = (TransitionType)EditorGUILayout.EnumPopup(trans[i], popupSize);
                }

                var (fromStart, fromEnd) = fromGroups[i];

                LatexCharEditor.ShowGroup(fromRenderer.characters, fromStart, fromEnd, 32);

                if (trans[i] == TransitionType.Remove || toIndex == toGroups.Count)
                    continue;

                var (toStart, toEnd) = toGroups[toIndex];
                LatexCharEditor.ShowGroup(toRenderer.characters, toStart, toEnd, 32);
                toIndex++;
            }

            if (toIndex == toGroups.Count)
                return;

            Space();
            GUILayout.Label("Following groups won't be used");

            for (var i = toIndex; i < toGroups.Count; i++) {
                var (toStart, toEnd) = toGroups[i];
                GUILayout.Label($"Clip's group {i}");
                LatexCharEditor.ShowGroup(toRenderer.characters, toStart, toEnd, 32);
            }
        }
    }
}
