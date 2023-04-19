using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Primer.Timeline.Editor
{
    public class EditTimelineDialog : EditorWindow
    {
        public record TimelineEditValues(float seconds, float targetTime, bool useTargetTime = false, bool preserveClips = true);

        public static TimelineEditValues Show(string title, string description, float seconds, float targetTime)
        {
            return Show(title, description, new TimelineEditValues(seconds, targetTime));
        }

        private static TimelineEditValues Show(string title, string description, TimelineEditValues input)
        {
            var window = CreateInstance<EditTimelineDialog>();
            window.titleContent = new GUIContent(title);
            window.description = description;

            window.seconds = input.seconds;
            window.targetTime = input.targetTime;
            window.preserveClips = input.preserveClips;

            var output = null as TimelineEditValues;
            window.onComplete += values => output = values;

            window.ShowModal();
            return output;
        }


        #region internals
        public string description;
        private TimelineEditValues values;
        private Action<TimelineEditValues> onComplete;

        private float seconds;
        private float targetTime;
        private bool useTargetTime;
        private bool preserveClips = true;

        private bool initializedPosition;
        private bool shouldClose;

        private void Cancel()
        {
            onComplete?.Invoke(null);
            shouldClose = true;
        }

        private void Submit()
        {
            onComplete?.Invoke(new TimelineEditValues(seconds, targetTime, useTargetTime, preserveClips));
            shouldClose = true;
        }

        private void OnGUI()
        {
            // Check if Esc/Return have been pressed
            var @event = Event.current;

            if (@event.type == EventType.KeyDown) {
                if (@event.keyCode is KeyCode.Escape)
                    Cancel();
                else if (@event.keyCode is KeyCode.Return or KeyCode.KeypadEnter)
                    Submit();
            }

            if (shouldClose) {
                Close();
                return;
            }

            // Draw our control
            var rect = EditorGUILayout.BeginVertical();

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField(description);

            EditorGUILayout.Space(8);
            GUI.SetNextControlName("secondsInput");
            seconds = EditorGUILayout.FloatField("Seconds", seconds);

            EditorGUILayout.Space(8);
            useTargetTime = EditorGUILayout.Toggle("Use target time instead", useTargetTime);
            GUI.SetNextControlName("targetTimeInput");
            targetTime = EditorGUILayout.FloatField("Target Time", targetTime);

            EditorGUILayout.Space(8);
            preserveClips = EditorGUILayout.Toggle("Preserve clips", preserveClips);
            EditorGUILayout.Space(12);

            // Draw OK / Cancel buttons
            var buttonsRect = EditorGUILayout.GetControlRect();
            buttonsRect.width /= 2;

            if (GUI.Button(buttonsRect, "Ok"))
                Submit();

            buttonsRect.x += buttonsRect.width;

            if (GUI.Button(buttonsRect, "Cancel"))
                Cancel();

            EditorGUILayout.Space(8);
            EditorGUILayout.EndVertical();

            // Force change size of the window
            if (rect.width != 0 && minSize != rect.size) {
                minSize = maxSize = rect.size;
            }

            if (initializedPosition)
                return;

            // Set dialog position next to mouse position
            var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            position = new Rect(mousePos.x + 32, mousePos.y, position.width, position.height);
            GUI.FocusControl("secondsInput");
            initializedPosition = true;
        }
        #endregion
    }
}
