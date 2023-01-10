using System;
using System.Collections.Generic;
using System.Linq;
using Primer.Animation;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Primer.Axis
{
    [ExecuteInEditMode]
    public class AxisRenderer : ObjectGenerator
    {
        // Internal game object containers
        private PrimerText2 labelObject;
        private PrimerBehaviour originArrow;
        private PrimerBehaviour endArrow;
        private readonly List<Tic2> tics = new();

        #region Inspectable config
        public bool hidden;
        public ArrowPresence arrowPresence = ArrowPresence.Both;
        public float min;
        public float max = 10;
        [Min(0.1f)] public float length = 1;
        [FormerlySerializedAs("thinkness")]
        public float thickness = 1;

        [Header("Label")]
        public string label = "Label";
        public Vector3 labelOffset = Vector3.zero;
        public AxisLabelPosition labelPosition = AxisLabelPosition.End;

        [Header("Required elements")]
        public Transform rod;

        [Header("Tics")]
        public bool showTics = true;
        [Tooltip("Ensures no more ticks are rendered.")]
        [Min(0)] public float ticStep = 2;
        [Range(1, 100)] public int maxTics = 50;
        public List<TicData> manualTics = new();
        #endregion

        #region Memory
        private ArrowPresence lastArrowPresence = ArrowPresence.Neither;
        #endregion

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
            labelObject = null;
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
            if (!labelObject) {
                labelObject = Create(primerTextPrefab);
                labelObject.transform.ScaleUpFromZero();
            }

            var labelPos = Vector3.zero;

            if (labelPosition == AxisLabelPosition.Along) {
                labelPos = new Vector3(length / 2 + offset, -2 * ticLabelDistance, 0f);
            }
            else if (labelPosition == AxisLabelPosition.End) {
                labelPos = new Vector3(length + offset + ticLabelDistance * 1.1f, 0f, 0f);
            }

            labelObject.transform.localPosition = labelPos + labelOffset;
            labelObject.text = label;
            labelObject.alignment = TextAlignmentOptions.Midline;
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
                endArrow = Create(arrowPrefab, Quaternion.Euler(0f, 90f, 0f)).GetPrimer();
                endArrow.ScaleUpFromZero();
            }

            if (arrowPresence == ArrowPresence.Positive) {
                originArrow?.ShrinkAndDispose();
                originArrow = null;
                return;
            }

            if (!originArrow) {
                originArrow = Create(arrowPrefab, Quaternion.Euler(0f, -90f, 0f)).GetPrimer();
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
            var step = (float)Math.Round(ticStep, 2);

            if (step <= 0) {
                return calculated;
            }

            for (var i = step; i <= max; i += step)
                calculated.Add(new TicData(i));

            for (var i = -step; i >= min; i -= step)
                calculated.Add(new TicData(i));

            return calculated;
        }

        #region Config
        private IAxisConfig configCache;
        private IAxisConfig config {
            get {
                if (configCache is not null)
                    return configCache;

                var sibling = transform.parent.GetComponent<IAxisConfig>();

                if (sibling == null) {
                    throw new Exception(
                        $"{nameof(AxisRenderer)} requires a sibling component that extends {nameof(IAxisConfig)}"
                    );
                }

                configCache = sibling;
                return sibling;
            }
        }

        private PrimerText2 primerTextPrefab => config.primerTextPrefab;
        private Transform arrowPrefab => config.arrowPrefab;
        private Tic2 ticPrefab => config.ticPrefab;
        private float paddingFraction => config.paddingFraction;
        private float ticLabelDistance => config.ticLabelDistance;
        #endregion
    }
}
