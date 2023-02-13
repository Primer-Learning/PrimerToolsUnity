using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Primer.Axis
{
    [HideLabel]
    [Serializable]
    [InlineProperty]
    [DisableContextMenu]
    [HideReferenceObjectPicker]
    [Title("Label")]
    internal class AxisLabel
    {
        public const float X_OFFSET = 0.4f;

        private PrimerText2 labelObject;

        public string text = "Label";
        public float fontSize = 2;
        public Vector3 offset = Vector3.zero;
        public Quaternion rotate = Quaternion.identity;
        public AxisLabelPosition position = AxisLabelPosition.End;

        public void Update(ChildrenDeclaration modifier, AxisDomain domain, float labelDistance)
        {
            modifier.Next(ref labelObject, "Label");

            var pos = position switch {
                AxisLabelPosition.Along => new Vector3(domain.length / 2, 0f, 0f),
                AxisLabelPosition.End => new Vector3(domain.rodEnd + X_OFFSET, 0f, 0f),
                _ => Vector3.zero,
            };

            labelObject.text = text;
            labelObject.fontSize = fontSize;
            labelObject.alignment = TextAlignmentOptions.Midline;

            var transform = labelObject.transform;
            transform.localPosition = pos + offset;

            if (rotate == Quaternion.identity)
                return;

            transform.localRotation = rotate;
            labelObject.forceOrientation = false;
        }
    }
}
