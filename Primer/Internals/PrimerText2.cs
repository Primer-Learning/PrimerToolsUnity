using TMPro;
using UnityEngine;

namespace Primer
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(TextMeshPro))]
    public class PrimerText2 : PrimerBehaviour
    {
        const string GAME_OBJECT_NAME_PREFIX = "@";

        [Multiline]
        public string text = "Primer";
        public float fontSize = 12;
        public Color color = Color.white;
        public TextAlignmentOptions alignment = TextAlignmentOptions.Center;

        TextMeshPro meshCache;
        TextMeshPro TextMeshPro => meshCache ??= GetComponent<TextMeshPro>();

        float numericValue = 0;
        public float AsNumber
        {
            get => numericValue;
            set {
                numericValue = value;
                text = Presentation.FormatNumber(value);
                TextMeshPro.text = text;
            }
        }

        void OnValidate() {
            var mesh = TextMeshPro;
            mesh.text = text;
            mesh.color = color;
            mesh.fontSize = fontSize;
            mesh.alignment = alignment;

            UpdateGameObjectName();
        }

        void UpdateGameObjectName() {
            if (name == "PrimerLabel" || name.StartsWith(GAME_OBJECT_NAME_PREFIX)) {
                var chunk = text.Length > 15 ? text[..15] : text;
                name = $"{GAME_OBJECT_NAME_PREFIX}{chunk}";
            }
        }
    }
}
