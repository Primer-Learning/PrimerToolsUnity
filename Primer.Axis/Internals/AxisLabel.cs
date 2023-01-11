using TMPro;
using UnityEngine;

namespace Primer.Axis
{
    internal class AxisLabel
    {
        private readonly AxisRenderer axis;
        private PrimerText2 label;

        public AxisLabel(AxisRenderer axis) => this.axis = axis;

        public void Update(ChildrenModifier modifier)
        {
            label = modifier.Next(label)
                .Called("Label")
                .WithComponent<PrimerText2>();

            var position = axis.labelPosition switch {
                AxisLabelPosition.Along => new Vector3(axis.length / 2 + axis.offset, -2 * axis.ticLabelDistance, 0f),
                AxisLabelPosition.End => new Vector3(axis.length + axis.offset + axis.ticLabelDistance * 1.1f, 0f, 0f),
                _ => Vector3.zero,
            };

            label.text = axis.label;
            label.alignment = TextAlignmentOptions.Midline;
            label.transform.localPosition = position + axis.labelOffset;
        }
    }
}
