using UnityEditor;

namespace Primer.Editor
{
    public record EditorHelpBox(string Message, MessageType MessageType = MessageType.None)
    {
        public static EditorHelpBox Info(string message) => new(message, MessageType.Info);
        public static EditorHelpBox Warning(string message) => new(message, MessageType.Warning);
        public static EditorHelpBox Error(string message) => new(message, MessageType.Error);

        public void Render() => EditorGUILayout.HelpBox(Message, MessageType);
    }
}
