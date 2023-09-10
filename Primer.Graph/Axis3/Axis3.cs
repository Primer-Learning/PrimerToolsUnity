using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Codice.CM.Common.Tree;
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
        public float rangeSize => Mathf.Max(0.001f, max - min);
        
        public float length = 1;
        private float _padding = 0.1f;
        public float padding {
            get => Mathf.Min(_padding, length / 2);
            set => _padding = value;
        }
        
        public float scale => lengthMinusPadding / rangeSize;
        
        public float lengthMinusPadding => length - padding;
        float thickness = 1;

        public Tween Transition()
        {
            var (addTics, updateTics, removeTics) = TransitionTics();

            var removeTween = Tween.Parallel(
                removeTics
            );
            // Rod, arrow, label transitions
            var updateTween = Tween.Parallel(
                updateTics,
                TransitionRod(),
                TransitionArrows(),
                TransitionLabel()
            );
            var addTween = Tween.Parallel(
                addTics
            );

            return Tween.Series(
                removeTween,
                updateTween,
                addTween
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
            var endArrowPos = new Vector3(length - padding, 0f, 0f);
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

        #region Tics

        public int ticStep = 2;
        public bool showZero;
        public int labelNumberOffset;
        public AxisTic ticPrefab;
        
        private List<TicData> PrepareTics()
        {
            if (ticStep <= 0)
                return new List<TicData>();

            return CalculateTics();
        }
        private List<TicData> CalculateTics()
        {
            var domain = this;
            var calculated = new List<TicData>();

            if (showZero)
                calculated.Add(new TicData(0, labelNumberOffset));

            for (var i = Mathf.Max(ticStep, domain.min); i <= domain.max; i += ticStep)
                calculated.Add(new TicData(i, labelNumberOffset));

            for (var i = Mathf.Min(-ticStep, domain.max); i >= domain.min; i -= ticStep)
                calculated.Add(new TicData(i, labelNumberOffset));

            return calculated;
        }
        private (Tween add, Tween update, Tween remove) TransitionTics()
        {
            var parentGnome = new SimpleGnome(transform);
            var gnomeTransform = parentGnome
                .Add("Tics container");
            gnomeTransform.localRotation = Quaternion.identity;
            var gnome = new SimpleGnome(gnomeTransform);

            var addTweens = new List<Tween>();
            var updateTweens = new List<Tween>();

            Vector3 GetPosition(AxisTic tic) => new(tic.value * scale, 0, 0);

            var ticsToRemove = new List<Transform>(gnome.activeChildren.ToList());
            
            foreach (var data in PrepareTics()) {
                var ticName = $"Tic {data.label}";
                var existingTic = gnome.transform.Find(ticName);
                AxisTic tic;

                // If the tic exists and is active already
                if (existingTic is not null && existingTic.gameObject.activeSelf)
                {
                    ticsToRemove.Remove(existingTic);
                    tic = existingTic.GetComponent<AxisTic>();
                    tic.transform.localRotation = Quaternion.identity;
                    updateTweens.Add(tic.MoveTo(GetPosition(tic)));
                    // Probably already scale 1, but just in case
                    // addTweens.Add(tic.ScaleTo(1));
                }
                else // The tic should exist but doesn't
                {
                    tic = gnome.Add<AxisTic>(ticPrefab.gameObject, ticName);
                    tic.value = data.value;
                    tic.label = data.label;
                    tic.transform.localPosition = GetPosition(tic);
                    tic.transform.localRotation = Quaternion.identity;
                    tic.transform.SetScale(0);
                    addTweens.Add(tic.ScaleTo(1));
                }
            }

            var removeTweens = ticsToRemove
                .Select(x => x.GetComponent<AxisTic>())
                .OrderByDescending(x => Mathf.Abs(x.value))
                .Select(
                    tic => {
                        updateTweens.Add(tic.MoveTo(GetPosition(tic)));
                        return tic.ScaleTo(0);
                    }
                );
            
            return (
                addTweens.RunInParallel(),
                updateTweens.RunInParallel(),
                removeTweens.RunInParallel()
            );
        }
        
        [Serializable]
        public class TicData
        {
            public float value;
            public string label;

            public TicData(float value, int labelOffset) {
                this.value = value;
                label = (value + labelOffset).FormatNumberWithDecimals();
            }
        }

        #endregion
        
        public void UpdateChildren()
        {
            using var tween = Transition();
            tween?.Apply();
        }
    }
}