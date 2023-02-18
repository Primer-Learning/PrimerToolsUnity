using System;
using UnityEditor;
using UnityEngine;

namespace Primer.Timeline.Editor
{
    public class EditorInputDialog : EditorWindow
    {
        public static string Show(string title, string description, string value, string okText = "OK", string cancelText = "Cancel")
        {
            var window = CreateInstance<EditorInputDialog>();
            window.titleContent = new GUIContent(title);
            window.description = description;
            window.value = value;
            window.okButton = okText;
            window.cancelButton = cancelText;
            window.onOkButton += () => value = window.value;
            window.ShowModal();
            return value;
        }

        public static int? Show(string title, string description, int? value, string okText = "OK", string cancelText = "Cancel")
        {
            var input = Show(title, description, value.HasValue ? value.Value.ToString() : null, okText, cancelText);
            return int.TryParse(input, out var result) ? result : null;
        }

        public static float? Show(string title, string description, float? value, string okText = "OK", string cancelText = "Cancel")
        {
            var input = Show(title, description, value.HasValue ? value.Value.ToString() : null, okText, cancelText);
            return float.TryParse(input, out var result) ? result : null;
        }


        #region internals
        private string description;
        private string value;
        private string okButton;
        private string cancelButton;
        private bool initializedPosition = false;
        private bool shouldClose = false;
        private Action onOkButton;

        private void OnGUI()
        {
            // Check if Esc/Return have been pressed
            var @event = Event.current;

            if (@event.type == EventType.KeyDown) {
                switch (@event.keyCode) {
                    case KeyCode.Escape:
                        shouldClose = true;
                        break;

                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        onOkButton?.Invoke();
                        shouldClose = true;
                        break;
                }
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
            GUI.SetNextControlName("inText");
            value = EditorGUILayout.TextField("", value);
            GUI.FocusControl("inText");
            EditorGUILayout.Space(12);

            // Draw OK / Cancel buttons
            var buttonsRect = EditorGUILayout.GetControlRect();
            buttonsRect.width /= 2;

            if (GUI.Button(buttonsRect, okButton)) {
                onOkButton?.Invoke();
                shouldClose = true;
            }

            buttonsRect.x += buttonsRect.width;

            if (GUI.Button(buttonsRect, cancelButton)) {
                value = null;
                shouldClose = true;
            }

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
            initializedPosition = true;
        }
        #endregion
    }
}
