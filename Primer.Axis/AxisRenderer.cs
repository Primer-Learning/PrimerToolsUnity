using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer.Animation;
using TMPro;
using UnityEngine;

namespace Primer.Axis
{
    [ExecuteAlways]
    public class AxisRenderer : MonoBehaviour
    {
        [Header("Prefabs")]
        public Transform arrowPrefab;
        public AxisTick tickPrefab;

        [Header("Domain")]
        public float min;
        public float max = 10;
        [Min(0.1f)] public float length = 1;

        [Header("Rod")]
        public Transform rod;
        public float thickness = 1;

        [Header("Label")]
        public string label = "Label";
        public Vector3 labelOffset = Vector3.zero;
        public AxisLabelPosition labelPosition = AxisLabelPosition.End;

        [Header("Arrows")]
        public ArrowPresence arrowPresence = ArrowPresence.Both;
        public float paddingFraction = 0.05f;

        [Header("Tics")]
        public bool showTics = true;
        [Min(0)] public float ticStep = 2;
        [Range(1, 100)] public int maxTics = 50;
        public float ticLabelDistance = 0.25f;
        public List<TicData> manualTics = new();


        internal float positionMultiplier => length * (1 - 2 * paddingFraction) / (max - min);
        internal float offset => -length * paddingFraction + min * positionMultiplier;


        public float DomainToPosition(float domainValue) => domainValue * positionMultiplier;


        private void OnEnable() => UpdateChildren();

        private void OnValidate() => UpdateChildren();


        public void UpdateChildren()
        {
            if (gameObject.IsPreset())
                return;

            var children = new ChildrenModifier(transform) {
                onCreate = x => x.GetPrimer().ScaleUpFromZero().Forget(),
                onRemove = x => x.GetPrimer().ShrinkAndDispose(),
            };

            // Rod is not a generated object
            children.NextMustBe(rod);

            if (isActiveAndEnabled) {
                rod.localPosition = new Vector3(offset, 0f, 0f);
                rod.localScale = new Vector3(length, thickness, thickness);

                UpdateLabel(children);
                UpdateArrows(children);
                UpdateTicks(children);
            }

            children.Apply();
        }


        #region UpdateLabel()
        private PrimerText2 labelObject;

        public void UpdateLabel(ChildrenModifier modifier)
        {
            labelObject = modifier.Next(labelObject)
                .Called("Label")
                .WithComponent<PrimerText2>();

            var position = labelPosition switch {
                AxisLabelPosition.Along => new Vector3(length / 2 + offset, -2 * ticLabelDistance, 0f),
                AxisLabelPosition.End => new Vector3(length + offset + ticLabelDistance * 1.1f, 0f, 0f),
                _ => Vector3.zero,
            };

            labelObject.text = label;
            labelObject.alignment = TextAlignmentOptions.Midline;
            labelObject.transform.localPosition = position + labelOffset;
        }
        #endregion


        #region UpdateArrows()
        private ArrowPresence? lastPresence;
        private Transform originArrow;
        private Transform endArrow;

        public void UpdateArrows(ChildrenModifier modifier)
        {
            FindOrCreateArrows(modifier);

            if (endArrow)
                endArrow.transform.localPosition = new Vector3(length + offset, 0f, 0f);

            if (originArrow)
                originArrow.transform.localPosition = new Vector3(offset, 0f, 0f);
        }

        private void FindOrCreateArrows(ChildrenModifier modifier)
        {
            if (arrowPresence == lastPresence) {
                modifier.NextMustBe(originArrow);
                modifier.NextMustBe(endArrow);
                return;
            }

            lastPresence = arrowPresence;

            if (arrowPresence == ArrowPresence.Neither) {
                endArrow = null;
                originArrow = null;
                return;
            }

            endArrow = modifier.Next(endArrow)
                .Called("End Arrow")
                .Initialize(x => x.localRotation = Quaternion.Euler(0f, 90f, 0f))
                .InstantiatedFrom(arrowPrefab);

            if (arrowPresence == ArrowPresence.Positive) {
                originArrow = null;
                return;
            }

            originArrow = modifier.Next(originArrow)
                .Called("Origin Arrow")
                .Initialize(x => x.localRotation = Quaternion.Euler(0f, -90f, 0f))
                .InstantiatedFrom(arrowPrefab);
        }
        #endregion


        #region UpdateTicks()
        public void UpdateTicks(ChildrenModifier modifier)
        {
            if (!showTics || ticStep <= 0)
                return;

            var expectedTicks = manualTics.Count != 0
                ? manualTics
                : CalculateTics();

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if ((maxTics > 0) && (expectedTicks.Count > maxTics)) {
                // TODO: reduce amount of tics in a smart way
                expectedTicks = expectedTicks.Take(maxTics).ToList();
            }

            foreach (var data in expectedTicks) {
                var tick = modifier.Next()
                    .Called($"Tick {data.label}")
                    .InstantiatedFrom(tickPrefab);

                tick.label = data.label;
                tick.transform.localPosition = new Vector3(data.value * positionMultiplier, 0, 0);
            }
        }

        private List<TicData> CalculateTics()
        {
            var calculated = new List<TicData>();
            var step = Mathf.Round(ticStep * 100) / 100;

            if (step <= 0)
                return calculated;

            for (var i = step; i <= max; i += step)
                calculated.Add(new TicData(i));

            for (var i = -step; i >= min; i -= step)
                calculated.Add(new TicData(i));

            return calculated;
        }
        #endregion
    }
}
