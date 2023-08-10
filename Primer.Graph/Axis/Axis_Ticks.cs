using System;
using System.Collections.Generic;
using System.Linq;
using Primer.Animation;
using Primer.Timeline;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Primer.Graph
{
    public partial class Axis
    {
        [Title("Ticks")]
        [FormerlySerializedAs("_showTicks")]
        public bool showTicks = true;

        [EnableIf(nameof(showTicks))]
        [FormerlySerializedAs("_showZero")]
        public bool showZero;

        [EnableIf(nameof(showTicks))]
        [FormerlySerializedAs("_lockTickOrientation")]
        public Optional<Direction> lockTickOrientation = Direction.Front;

        [MinValue(0.1f)]
        [EnableIf(nameof(showTicks))]
        [DisableIf("@manualTicks.Count != 0")]
        [FormerlySerializedAs("_step")]
        public float step = 2;

        [PropertyRange(1, 100)]
        [EnableIf(nameof(showTicks))]
        [FormerlySerializedAs("_maxTicks")]
        public int maxTicks = 50;

        [PropertyRange(0, 10)]
        [EnableIf(nameof(showTicks))]
        [FormerlySerializedAs("_maxDecimals")]
        public int maxDecimals = 2;

        [EnableIf(nameof(showTicks))]
        [FormerlySerializedAs("_tickOffset")]
        public float tickOffset;

        [EnableIf(nameof(showTicks))]
        [FormerlySerializedAs("_labelNumberOffset")]
        public int labelNumberOffset;

        [EnableIf(nameof(showTicks))]
        [FormerlySerializedAs("_valuePositionOffset")]
        public float valuePositionOffset;

        [EnableIf(nameof(showTicks))]
        [FormerlySerializedAs("_manualTicks")]
        public List<TickData> manualTicks;

        [RequiredIn(PrefabKind.PrefabAsset)]
        [EnableIf(nameof(showTicks))]
        [FormerlySerializedAs("_tickPrefab")]
        public PrefabProvider<AxisTick> tickPrefab;


        private List<TickData> PrepareTicks()
        {
            if (!showTicks || step <= 0 || tickPrefab.isEmpty)
                return new List<TickData>();

            var expectedTicks = manualTicks.Count != 0
                ? manualTicks
                : CalculateTics();

            return CropTicksCount(expectedTicks);
        }

        private (Tween add, Tween update, Tween remove) TransitionTicks(Gnome parentGnome, bool defer)
        {
            var gnome = parentGnome
                .AddGnome("Ticks container")
                .SetDefaults();

            var addTweens = new List<Tween>();
            var updateTweens = new List<Tween>();

            Vector3 GetPosition(AxisTick tick) => new((tick.value + valuePositionOffset) * scale, tickOffset, 0);

            foreach (var data in PrepareTicks()) {
                var tick = gnome.Add(tickPrefab, $"Tick {data.label}");
                tick.value = data.value;
                tick.label = data.label;

                if (gnome.IsCreated(tick)) {
                    tick.transform.localPosition = GetPosition(tick);
                    tick.transform.SetScale(0);
                    addTweens.Add(tick.ScaleTo(1, 0));
                }
                else {
                    updateTweens.Add(tick.MoveTo(GetPosition(tick)));
                }

                if (lockTickOrientation.enabled)
                    lockTickOrientation.value.ApplyTo(tick.latex);
            }

            var removeTweens = gnome.ManualPurge(defer: true)
                .Select(x => x.GetComponent<AxisTick>())
                .OrderByDescending(x => Mathf.Abs(x.value))
                .Select(
                    tick => {
                        updateTweens.Add(tick.MoveTo(GetPosition(tick)));

                        return tick.ScaleTo(0, 1)
                            .Observe(onDispose: () => tick.Dispose(defer));
                    }
                );

            return (
                addTweens.RunInParallel(delayBetweenStarts: 0.05f).WithDuration(Tween.DEFAULT_DURATION),
                updateTweens.RunInParallel(),
                removeTweens
                    .RunInParallel(delayBetweenStarts: 0.05f)
                    .WithDuration(Tween.DEFAULT_DURATION)
                    .Observe(onDispose: () => gnome.Purge(defer))
            );
        }

        private List<TickData> CropTicksCount(List<TickData> ticks)
        {
            if (maxTicks <= 0 || ticks.Count <= maxTicks)
                return ticks;

            var pickIndexes = ticks.Count / maxTicks + 1;

            return ticks
                .Where((_, i) => i % pickIndexes == 0)
                .Take(maxTicks - 1)
                .Append(ticks.Last())
                .ToList();
        }

        private List<TickData> CalculateTics()
        {
            var domain = this;
            var calculated = new List<TickData>();
            var multiplier = Mathf.Pow(10, maxDecimals);
            var roundedStep = Mathf.Round(step * multiplier) / multiplier;

            if (roundedStep <= 0)
                return calculated;

            if (showZero)
                calculated.Add(new TickData(0, labelNumberOffset));

            for (var i = Mathf.Max(roundedStep, domain.min); i <= domain.max; i += roundedStep)
                calculated.Add(new TickData(i, labelNumberOffset));

            for (var i = Mathf.Min(-roundedStep, domain.max); i >= domain.min; i -= roundedStep)
                calculated.Add(new TickData(i, labelNumberOffset));

            return calculated;
        }

        [Serializable]
        public class TickData
        {
            public float value;
            public string label;

            public TickData(float value, int labelOffset) {
                this.value = value;
                label = (value + labelOffset).FormatNumberWithDecimals();
            }
        }
    }
}
