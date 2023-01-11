using TMPro;
using UnityEngine;

namespace Primer
{
    [ExecuteAlways]
    [RequireComponent(typeof(TextMeshPro))]
    public class PrimerText2 : MonoBehaviour
    {
        [Multiline]
        public string text = "";
        public float fontSize = 2;
        public Color color = Color.white;
        public TextAlignmentOptions alignment = TextAlignmentOptions.Center;
        public Vector3 orientation = Vector3.forward;


        private TextMeshPro meshCache;
        private TextMeshPro textMeshPro => meshCache ??= GetComponent<TextMeshPro>();


        private float numericValue;
        public float AsNumber {
            get => numericValue;
            set {
                numericValue = value;
                text = Presentation.FormatNumber(value);
                textMeshPro.text = text;
            }
        }


        private void OnValidate() => UpdateMesh();

        private void Update()
        {
            UpdateMesh();
            transform.LookAt(transform.position + orientation);
        }


        private void UpdateMesh()
        {
            var mesh = textMeshPro;
            mesh.text = text;
            mesh.color = color;
            mesh.fontSize = fontSize;
            mesh.alignment = alignment;
        }
    }
}
