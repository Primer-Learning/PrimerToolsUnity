using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Primer
{
    [ExecuteAlways]
    [RequireComponent(typeof(TextMeshPro))]
    public class PrimerText2 : MonoBehaviour
    {
        private TextMeshPro meshCache;
        private TextMeshPro textMeshPro => meshCache ??= GetComponent<TextMeshPro>();


        public bool forceOrientation = true;

        [EnableIf(nameof(forceOrientation))]
        public Vector3 orientation = Vector3.forward;

        [ShowInInspector]
        [HideLabel]
        [PropertyOrder(-1)]
        [PropertySpace(SpaceBefore = 16, SpaceAfter = 16)]
        [MultiLineProperty(5)]
        public string text {
            get => textMeshPro.text;
            set => textMeshPro.text = value;
        }

        [ShowInInspector]
        public float fontSize {
            get => textMeshPro.fontSize;
            set => textMeshPro.fontSize = value;
        }

        [ShowInInspector]
        public TextAlignmentOptions alignment {
            get => textMeshPro.alignment;
            set => textMeshPro.alignment = value;
        }

        [ShowInInspector]
        public Color color {
            get => textMeshPro.color;
            set => textMeshPro.color = value;
        }


        private float numericValue;
        public float AsNumber {
            get => numericValue;
            set {
                numericValue = value;
                text = value.FormatNumber();
            }
        }


        private void Update()
        {
            if (forceOrientation)
                transform.LookAt(transform.position + orientation);
        }
    }
}
