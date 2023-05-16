using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Primer.Axis
{
    [HideLabel]
    [Serializable]
    [InlineProperty]
    [DisableContextMenu]
    [HideReferenceObjectPicker]
    [Title("Ticks")]
    public class AxisTicks
    {
        public bool showTicks = true;

        [EnableIf("showTicks")]
        public bool showZero;

        [MinValue(0.1f)]
        [EnableIf("showTicks")]
        [DisableIf("@manualTicks.Count != 0")]
        public float step = 2;

        [Range(1, 100)]
        [EnableIf("showTicks")]
        public int maxTicks = 50;

        [Range(0, 10)]
        [EnableIf("showTicks")]
        public int maxDecimals = 2;

        [FormerlySerializedAs("offset")] [EnableIf("showTicks")]
        public float verticalOffset = 0;

        [EnableIf("showTicks")]
        public int labelNumberOffset = 0;
        
        [EnableIf("showTicks")]
        public float valuePositionOffset = 0;
        
        [EnableIf("showTicks")]
        public List<TicData> manualTicks = new();

        [RequiredIn(PrefabKind.PrefabAsset)]
        [EnableIf("showTicks")]
        public PrefabProvider<AxisTick> prefab;


        public void Update(ChildrenDeclaration modifier, AxisDomain domain)
        {
            if (!showTicks || step <= 0 || prefab.isEmpty)
                return;

            var expectedTicks = manualTicks.Count != 0
                ? manualTicks
                : CalculateTics(domain);

            foreach (var data in CropTicksCount(expectedTicks)) {
                var tick = modifier.NextIsInstanceOf(prefab, $"Tick {data.label}");
                tick.label = data.label;
                tick.transform.localPosition = new Vector3((data.value + valuePositionOffset) * domain.scale, verticalOffset, 0);
            }
        }

        private List<TicData> CropTicksCount(List<TicData> ticks)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (maxTicks <= 0 || ticks.Count <= maxTicks)
                return ticks;

            var picIndexes = ticks.Count / maxTicks + 1;

            var copy = ticks
                .Where((_, i) => i % picIndexes == 0)
                .Take(maxTicks - 1)
                .ToList();

            copy.Add(ticks.Last());
            return copy;
        }

        private List<TicData> CalculateTics(AxisDomain domain)
        {
            var calculated = new List<TicData>();
            var multiplier = Mathf.Pow(10, maxDecimals);
            var roundedStep = Mathf.Round(step * multiplier) / multiplier;

            if (roundedStep <= 0)
                return calculated;

            if (showZero)
                calculated.Add(new TicData(0, labelNumberOffset));

            for (var i = Mathf.Max(roundedStep, domain.min); i <= domain.max; i += roundedStep)
                calculated.Add(new TicData(i, labelNumberOffset));

            for (var i = Mathf.Min(-roundedStep, domain.max); i >= domain.min; i -= roundedStep)
                calculated.Add(new TicData(i, labelNumberOffset));

            return calculated;
        }
    }
}
