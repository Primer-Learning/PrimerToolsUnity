using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;

namespace Primer.Timeline.Editor
{
    [CustomEditor(typeof(TriggerMarker))]
    public class TriggerMarkerEditor : UnityEditor.Editor
    {
        private TriggerMarker marker => target as TriggerMarker;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var director = TimelineEditor.inspectedDirector;
            var track = marker.parent;
            var bound = director.GetGenericBinding(track);

            var animation = bound as TriggeredBehaviour
                ?? (bound as Component)?.GetComponent<TriggeredBehaviour>()
                ?? (bound as GameObject)?.GetComponent<TriggeredBehaviour>();

            if (animation is null) {
                EditorGUILayout.HelpBox(
                    $"{nameof(TriggerMarker)} must be in a track bound to a {nameof(TriggeredBehaviour)}",
                    MessageType.Error
                );

                return;
            }

            marker.method = MethodSelector.Render(marker.method, MethodSelector.GetMethodsWithNoParams(animation));

            if (marker.method is null)
                return;

            // display the method name as a label
            var regex = new Regex("[A-Z]", RegexOptions.Compiled);
            var text = regex.Replace(marker.method, " $0").Trim();

            GUILayout.Space(32);
            GUILayout.Label(text, new GUIStyle {
                fontSize = 16,
                normal = { textColor = Color.white },
                wordWrap = true,
            });
        }
    }
}
