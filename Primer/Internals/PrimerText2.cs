using TMPro;
using UnityEngine;

namespace Primer
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(TextMeshPro))]
    public class PrimerText2 : PrimerBehaviour
    {
        const string GAME_OBJECT_NAME_PREFIX = "txt";

        [Multiline]
        public string text = "Primer";
        public float fontSize = 12;
        public Color color = Color.white;
        public Vector3 orientation = Vector3.forward;
        public TextAlignmentOptions alignment = TextAlignmentOptions.Center;

        TextMeshPro meshCache;
        TextMeshPro textMeshPro => meshCache ??= GetComponent<TextMeshPro>();

        float numericValue = 0;
        public float asNumber
        {
            get => numericValue;
            set {
                numericValue = value;
                text = Presentation.FormatNumber(value);
                textMeshPro.text = text;
            }
        }

        void OnValidate() {
            var mesh = textMeshPro;
            mesh.text = text;
            mesh.color = color;
            mesh.fontSize = fontSize;
            mesh.alignment = alignment;

            transform.LookAt(transform.position + orientation);

            var name = gameObject.name;

            if (name == "PrimerLabel" || name.StartsWith($"{GAME_OBJECT_NAME_PREFIX} ")) {
                gameObject.name = $"{GAME_OBJECT_NAME_PREFIX} {text}";
            }
        }
    }
}
