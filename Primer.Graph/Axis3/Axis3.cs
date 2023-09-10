using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Primer.Animation;
using Primer.Latex;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    public class Axis3 : MonoBehaviour
    {
        public float min = 0;
        public float max = 10;
        
        public float length = 1;
        
        private float _padding = 0.1f;
        public float padding {
            get => Mathf.Min(_padding, length / 2);
            set => _padding = value;
        }
        private float actualPadding => Mathf.Min(padding, length / 2);
        public float lengthMinusPadding => length - padding;
        float thickness = 1;

        public Tween Transition()
        {
            // Rod transition
            return Tween.Parallel(
                TransitionRod(),
                TransitionArrows(),
                TransitionLabel()
            );
        }

        #region Bar
        private Tween TransitionRod()
        {
            var rod = new SimpleGnome("Rod", parent: transform);
            var position = new Vector3(-padding, 0f, 0f);
            var scale = new Vector3(length, thickness, thickness);

            DrawBar(rod);

            return Tween.Parallel(
                rod.transform.localPosition == position ? null : rod.MoveTo(position),
                rod.transform.localScale == scale ? null : rod.ScaleTo(scale)
            );
        }

        private static void DrawBar(SimpleGnome rod)
        {
            var cylinder = rod.Add<Transform>("AxisRod", "AxisRod");
            cylinder.localPosition = new Vector3(0.5f, 0, 0);
            cylinder.localRotation = Quaternion.Euler(0, 0, -90);
            cylinder.localScale = new Vector3(0.0375f, 0.5f, 0.0375f);
        }
        #endregion

        #region Arrows
        [Title("Arrows")]
        public ArrowPresence arrowPresence = ArrowPresence.Both;
        public GameObject arrowPrefab;

        private Tween TransitionArrows()
        {
            if (arrowPresence == ArrowPresence.Neither)
                return null;

            var gnome = new SimpleGnome(transform);

            var endArrow = gnome.Add<Transform>(arrowPrefab, "End Arrow");
            endArrow.localRotation = Quaternion.Euler(0f, 90f, 0f);
            var endArrowPos = new Vector3(length - actualPadding, 0f, 0f);
            var endArrowTween = endArrowPos == endArrow.localPosition ? null : endArrow.MoveTo(endArrowPos);

            if (arrowPresence != ArrowPresence.Both)
                return endArrowTween;

            var originArrow = gnome.Add<Transform>(arrowPrefab, "Origin Arrow");
            originArrow.localRotation = Quaternion.Euler(0f, -90f, 0f);
            var originArrowPos = new Vector3(-padding, 0f, 0f);
            var originArrowTween = originArrowPos == originArrow.localPosition ? null : originArrow.MoveTo(originArrowPos);

            return Tween.Parallel(endArrowTween, originArrowTween);
        }

        #endregion

        #region Label

        public const float X_OFFSET = 0.4f;

        [Title("Label")]
        public bool showLabel = true;

        [EnableIf(nameof(showLabel))]
        public string label = "Label";

        public float labelSize = 1;
        
        [EnableIf(nameof(showLabel))]
        public Vector3 labelOffset = Vector3.zero;

        [EnableIf(nameof(showLabel))]
        public Quaternion labelRotation = Quaternion.identity;

        [EnableIf(nameof(showLabel))]
        public AxisLabelPosition labelPosition = AxisLabelPosition.End;

        private Tween TransitionLabel()
        {
            var gnome = new SimpleGnome(transform);
            var labelTransform = gnome.AddLatex(label, "Label").transform;

            var pos = labelOffset + (labelPosition switch {
                AxisLabelPosition.Along => new Vector3(length / 2, -0.3f, 0f),
                AxisLabelPosition.End => new Vector3(length - padding + X_OFFSET, 0f, 0f),
                _ => Vector3.zero,
            });

            return Tween.Parallel(
                pos == labelTransform.localPosition ? null : labelTransform.MoveTo(pos),
                labelRotation == labelTransform.localRotation ? null : labelTransform.RotateTo(labelRotation),
                showLabel ? labelTransform.ScaleTo(0.1f * labelSize) : labelTransform.ScaleTo(0f)
            );
        }

        #endregion
        
        public void UpdateChildren()
        {
            using var tween = Transition();
            tween?.Apply();
        }
    }
}