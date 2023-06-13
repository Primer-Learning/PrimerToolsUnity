using System;
using Primer.Latex;
using Sirenix.OdinInspector;
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

        public string text = "Label";
        public Vector3 offset = Vector3.zero;
        public Quaternion rotate = Quaternion.identity;
        public AxisLabelPosition position = AxisLabelPosition.End;


        public void Update(Container container, AxisDomain domain, float labelDistance)
        {
            var label = container.AddLatex(text, "Label");
            var transform = label.transform;

            var pos = position switch {
                AxisLabelPosition.Along => new Vector3(domain.length / 2, 0f, 0f),
                AxisLabelPosition.End => new Vector3(domain.rodEnd + X_OFFSET, 0f, 0f),
                _ => Vector3.zero,
            };

            transform.localPosition = pos + offset;
            transform.localRotation = rotate;
        }
    }
}
