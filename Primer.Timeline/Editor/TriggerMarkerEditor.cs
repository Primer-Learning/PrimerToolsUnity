using System.Linq;
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
        }
    }
}
