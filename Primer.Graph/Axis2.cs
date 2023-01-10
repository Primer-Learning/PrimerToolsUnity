using System.Collections.Generic;
using System.Linq;
using Primer.Animation;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Primer.Graph
{
    [ExecuteInEditMode]
    public class Axis2 : ObjectGenerator
    {
        // Internal game object containers
        private readonly List<Tic2> tics = new();
        public ArrowPresence arrowPresence = ArrowPresence.Both;
        private PrimerText2 axisLabel;
        private PrimerBehaviour endArrow;

        // Graph accessors
        private Graph2 graphCache;

        // Configuration values
        public bool hidden;

        [Header("Label")]
        public string label = "Label";
        public Vector3 labelOffset = Vector3.zero;
        public AxisLabelPosition labelPosition = AxisLabelPosition.End;

        // Memory
        private ArrowPresence lastArrowPresence = ArrowPresence.Neither;
        [Min(0.1f)] public float length = 1;
        public List<TicData> manualTics = new();
        public float max = 10;
        [Tooltip("Ensures no more ticks are rendered.")]
        [Range(1, 100)] public int maxTics = 50;
        public float min;
        private PrimerBehaviour originArrow;

        [Header("Required elements")]
        public Transform rod;

        [Header("Tics")]
        public bool showTics = true;
        [FormerlySerializedAs("thinkness")]
        public float thickness = 1;
        [Min(0)] public float ticStep = 2;
        private Graph2 graph => graphCache ??= transform.parent?.GetComponent<Graph2>();
        private PrimerText2 primerTextPrefab => graph.primerTextPrefab;
        private PrimerBehaviour arrowPrefab => graph.arrowPrefab;
        private Tic2 ticPrefab => graph.ticPrefab;
        private float paddingFraction => graph.paddingFraction;
        private float ticLabelDistance => graph.ticLabelDistance;

        // Calculated fields
        private float positionMultiplier => length * (1 - 2 * paddingFraction) / (max - min);
        private float offset => -length * paddingFraction + min * positionMultiplier;

        public float DomainToPosition(float domainValue, bool ignoreHidden = false)
            => !ignoreHidden && hidden ? 0 : domainValue * positionMultiplier;

        public override void UpdateChildren()
        {
            gameObject.SetActive(!hidden);

            if (hidden) {
                RemoveGeneratedChildren();
                return;
            }

            UpdateRod();
            UpdateLabel();
            UpdateArrowHeads();
            UpdateTics();
        }

        protected override void OnChildrenRemoved()
        {
            tics.Clear();
            axisLabel = null;
            originArrow = null;
            endArrow = null;
            lastArrowPresence = ArrowPresence.Neither;
        }

        private void UpdateRod()
        {
            rod.localPosition = new Vector3(offset, 0f, 0f);
            rod.localScale = new Vector3(length, thickness, thickness);
        }

        private void UpdateLabel()
        {
            if (!axisLabel) {
                axisLabel = Create(primerTextPrefab);
                axisLabel.transform.ScaleUpFromZero();
            }

            var labelPos = Vector3.zero;

            if (labelPosition == AxisLabelPosition.Along) {
                labelPos = new Vector3(length / 2 + offset, -2 * ticLabelDistance, 0f);
            }
            else if (labelPosition == AxisLabelPosition.End) {
                labelPos = new Vector3(length + offset + ticLabelDistance * 1.1f, 0f, 0f);
            }

            axisLabel.transform.localPosition = labelPos + labelOffset;
            axisLabel.text = label;
            axisLabel.alignment = TextAlignmentOptions.Midline;
        }

        private void UpdateArrowHeads()
        {
            if (arrowPresence != lastArrowPresence) {
                lastArrowPresence = arrowPresence;
                RecreateArrowHeads();
            }

            if (endArrow) {
                endArrow.transform.localPosition = new Vector3(length + offset, 0f, 0f);
            }

            if (originArrow) {
                originArrow.transform.localPosition = new Vector3(offset, 0f, 0f);
            }
        }

        private void RecreateArrowHeads()
        {
            if (arrowPresence == ArrowPresence.Neither) {
                endArrow?.ShrinkAndDispose();
                endArrow = null;
                originArrow?.ShrinkAndDispose();
                originArrow = null;
                return;
            }

            if (!endArrow) {
                endArrow = Create(arrowPrefab, Quaternion.Euler(0f, 90f, 0f));
                endArrow.ScaleUpFromZero();
            }

            if (arrowPresence == ArrowPresence.Positive) {
                originArrow?.ShrinkAndDispose();
                originArrow = null;
                return;
            }

            if (!originArrow) {
                originArrow = Create(arrowPrefab, Quaternion.Euler(0f, -90f, 0f));
                originArrow.ScaleUpFromZero();
            }
        }

        internal void UpdateTics()
        {
            var mustShowTics = showTics && (ticStep > 0);

            if (!mustShowTics) {
                foreach (var tic in tics) {
                    tic.ShrinkAndDispose();
                }

                tics.Clear();
                return;
            }

            var expectedTics = manualTics.Count != 0 ? manualTics : CalculateTics();

            if ((maxTics != 0) && (expectedTics.Count() > maxTics)) {
                // TODO: reduce amount of tics in a smart way
                expectedTics = expectedTics.Take(maxTics).ToList();
            }

            var (add, remove, update) = SynchronizeLists(
                expectedTics,
                tics,
                (data, tic) => tic.value == data.value
            );

            foreach (var (data, tic) in update) {
                tic.label = data.label;
            }

            foreach (var tic in remove) {
                tics.Remove(tic);

                if (tic)
                    tic.ShrinkAndDispose();
            }

            foreach (var data in add) {
                var newTic = Create(ticPrefab);
                newTic.Initialize(primerTextPrefab, data, ticLabelDistance);
                newTic.ScaleUpFromZero();
                tics.Add(newTic);
            }

            foreach (var tic in tics) {
                tic.transform.localPosition = new Vector3(tic.value * positionMultiplier, 0, 0);
            }
        }

        private List<TicData> CalculateTics()
        {
            var calculated = new List<TicData>();
            var step = (float)System.Math.Round(ticStep, 2);

            if (step <= 0) {
                return calculated;
            }

            for (var i = step; i <= max; i += step)
                calculated.Add(new TicData(i));

            for (var i = -step; i >= min; i -= step)
                calculated.Add(new TicData(i));

            return calculated;
        }
    }
}
