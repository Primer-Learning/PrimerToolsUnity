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

            var children = new ChildrenDeclaration(
                transform,
                onCreate: x => {
                    Debug.Log($"Created {x.gameObject.name}");
                    x.GetPrimer().ScaleUpFromZero().Forget();
                },
                onRemove: x => {
                    Debug.Log($"Removed {x.gameObject.name}");
                    x.GetPrimer().ShrinkAndDispose();
                }
            );

            // Rod is not a generated object
            children.NextIs(rod);

            if (enabled && isActiveAndEnabled) {
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

        public void UpdateLabel(ChildrenDeclaration modifier)
        {
            modifier.Next(ref labelObject, "Label");

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
        private Transform originArrow;
        private Transform endArrow;

        public void UpdateArrows(ChildrenDeclaration modifier)
        {
            if (arrowPresence == ArrowPresence.Neither)
                return;

            modifier.NextIsInstanceOf(
                prefab: arrowPrefab,
                cache: ref endArrow,
                name: "End Arrow",
                init: x => x.localRotation = Quaternion.Euler(0f, 90f, 0f)
            );

            endArrow.localPosition = new Vector3(length + offset, 0f, 0f);

            if (arrowPresence != ArrowPresence.Both)
                return;

            modifier.NextIsInstanceOf(
                prefab: arrowPrefab,
                cache: ref originArrow,
                name: "Origin Arrow",
                init: x => x.localRotation = Quaternion.Euler(0f, -90f, 0f)
            );

            originArrow.localPosition = new Vector3(offset, 0f, 0f);
        }
        #endregion


        #region UpdateTicks()
        public void UpdateTicks(ChildrenDeclaration modifier)
        {
            if (!showTics || ticStep <= 0)
                return;

            var expectedTicks = manualTics.Count != 0
                ? manualTics
                : CalculateTics();

            foreach (var data in CropTicksCount(expectedTicks)) {
                var tick = modifier.NextIsInstanceOf(tickPrefab, $"Tick {data.label}");
                tick.label = data.label;
                tick.transform.localPosition = new Vector3(data.value * positionMultiplier, 0, 0);
            }
        }

        private List<TicData> CropTicksCount(List<TicData> ticks)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (maxTics <= 0 || ticks.Count <= maxTics)
                return ticks;

            var picIndexes = ticks.Count / maxTics + 1;

            var copy = ticks
                .Where((_, i) => i % picIndexes == 0)
                .Take(maxTics - 1)
                .ToList();

            copy.Add(ticks.Last());
            return copy;
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
