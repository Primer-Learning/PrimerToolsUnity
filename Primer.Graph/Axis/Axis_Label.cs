using Primer.Animation;
using Primer.Latex;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Primer.Graph
{
    public partial class Axis
    {
        public const float X_OFFSET = 0.4f;

        [Title("Label")]
        [FormerlySerializedAs("_showLabel")]
        public bool showLabel = true;

        [EnableIf(nameof(showLabel))]
        [FormerlySerializedAs("_label")]
        public string label = "Label";

        [EnableIf(nameof(showLabel))]
        [FormerlySerializedAs("_labelOffset")]
        public Vector3 labelOffset = Vector3.zero;

        [EnableIf(nameof(showLabel))]
        [FormerlySerializedAs("_labelRotation")]
        public Quaternion labelRotation = Quaternion.identity;

        [EnableIf(nameof(showLabel))]
        [FormerlySerializedAs("_labelPosition")]
        public AxisLabelPosition labelPosition = AxisLabelPosition.End;

        private Tween TransitionLabel(Primer.SimpleGnome labelParent)
        {
            var labelTransform = labelParent.AddLatex(label, "Label").transform;

            var pos = labelOffset + (labelPosition switch {
                AxisLabelPosition.Along => new Vector3(length / 2, 0f, 0f),
                AxisLabelPosition.End => new Vector3(rodEnd + X_OFFSET, 0f, 0f),
                _ => Vector3.zero,
            });

            return Tween.Parallel(
                pos == labelTransform.localPosition ? null : labelTransform.MoveTo(pos),
                labelRotation == labelTransform.localRotation ? null : labelTransform.RotateTo(labelRotation)
            );
        }
    }
}
