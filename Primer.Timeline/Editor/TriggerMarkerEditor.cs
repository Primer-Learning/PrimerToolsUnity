using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;

namespace Primer.Timeline.Editor
{
    [CustomEditor(typeof(TriggerMarker))]
    public class TriggerMarkerEditor : UnityEditor.Editor
    {
        private static string[] ignoreMethods = {
            "IsInvoking",
            "CancelInvoke",
            "StopAllCoroutines",
            "GetComponent",
            "GetComponentInChildren",
            "GetComponentsInChildren",
            "GetComponentInParent",
            "GetComponentsInParent",
            "GetComponents",
            "GetInstanceID",
            "GetHashCode",
            "ToString",
            "GetType",
        };

        private TriggerMarker marker => target as TriggerMarker;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var director = TimelineEditor.inspectedDirector;
            var track = marker.parent;
            var bound = director.GetGenericBinding(track);

            if (bound is not TriggeredAnimation animation) {
                EditorGUILayout.HelpBox($"{nameof(TriggerMarker)} must be in a track bound to a {nameof(TriggeredAnimation)}", MessageType.Error);
                return;
            }

            var methods = animation.GetType()
                .GetMethods()
                .Where(IsValidMethod)
                .Select(x => x.Name)
                .ToArray();

            EditorGUI.BeginDisabledGroup(methods.Length == 1);
            var index = Mathf.Clamp(Array.IndexOf(methods, marker.method), 0, methods.Length);
            var labels = methods.Select(x => $"{x}()").ToArray();
            var newIndex = EditorGUILayout.Popup("Method to invoke", index, labels);
            EditorGUI.EndDisabledGroup();

            marker.method = methods[newIndex];
        }

        private static bool IsValidMethod(MethodInfo method) => (
            method.GetParameters().Length == 0
            && !method.Name.StartsWith("get_")
            && !ignoreMethods.Contains(method.Name)
        );
    }
}
