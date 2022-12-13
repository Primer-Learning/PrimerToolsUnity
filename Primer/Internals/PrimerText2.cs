using TMPro;
using UnityEngine;

namespace Primer
{
    [ExecuteAlways]
    [RequireComponent(typeof(TextMeshPro))]
    public class PrimerText2 : PrimerBehaviour
    {
        [Multiline]
        public string text = "Primer";
        public float fontSize = 12;
        public Color color = Color.white;
        public TextAlignmentOptions alignment = TextAlignmentOptions.Center;
        public Vector3 orientation = Vector3.forward;


        TextMeshPro meshCache;
        TextMeshPro textMeshPro => meshCache ??= GetComponent<TextMeshPro>();

        float numericValue = 0;
        public float AsNumber
        {
            get => numericValue;
            set {
                numericValue = value;
                text = Presentation.FormatNumber(value);
                textMeshPro.text = text;
            }
        }


        void OnValidate() => UpdateMesh();

        void Update()
        {
            UpdateMesh();
            transform.LookAt(transform.position + orientation);
        }


        void UpdateMesh() {
            var mesh = textMeshPro;
            mesh.text = text;
            mesh.color = color;
            mesh.fontSize = fontSize;
            mesh.alignment = alignment;
        }
    }
}
