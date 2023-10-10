using System;
using System.Collections.Generic;
using System.Linq;
using Primer.Animation;
using Primer.Timeline;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
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

        [FormerlySerializedAs("tickPrefab")]
        [RequiredIn(PrefabKind.PrefabAsset)]
        [EnableIf(nameof(showTicks))]
        [FormerlySerializedAs("_tickPrefab")]
        public AxisTic ticPrefab;


        private List<TickData> PrepareTicks()
        {
            if (!showTicks || step <= 0)
                return new List<TickData>();

            var expectedTicks = manualTicks.Count != 0
                ? manualTicks
                : CalculateTics();

            return CropTicksCount(expectedTicks);
        }

        private (Tween add, Tween update, Tween remove) TransitionTicks(Primer.SimpleGnome parentGnome, bool defer)
        {

            var gnomeTransform = parentGnome
                .Add("Ticks container");
            gnomeTransform.localRotation = Quaternion.identity;
            var gnome = new Primer.SimpleGnome(gnomeTransform);

            var addTweens = new List<Tween>();
            var updateTweens = new List<Tween>();

            Vector3 GetPosition(AxisTic tick) => new((tick.value + valuePositionOffset) * scale, tickOffset, 0);

            var ticsToRemove = new List<Transform>(gnome.activeChildren.ToList());
            
            foreach (var data in PrepareTicks()) {
                var tickName = $"Tick {data.label}";
                var existingTick = gnome.transform.Find(tickName);
                AxisTic tic;

                // If the tick exists and is active already
                if (existingTick is not null && existingTick.gameObject.activeSelf)
                {
                    ticsToRemove.Remove(existingTick);
                    tic = existingTick.GetComponent<AxisTic>();
                    tic.transform.localPosition = GetPosition(tic);
                    tic.transform.localRotation = Quaternion.identity;
                    updateTweens.Add(tic.MoveTo(GetPosition(tic)));
                    // Probably already scale 1, but just in case
                    // addTweens.Add(tick.ScaleTo(1));
                }
                else // The tick should exist but doesn't
                {
                    tic = gnome.Add<AxisTic>(ticPrefab.gameObject, tickName);
                    tic.value = data.value;
                    tic.label = data.label;
                    tic.transform.localPosition = GetPosition(tic);
                    tic.transform.localRotation = Quaternion.identity;
                    tic.transform.SetScale(0);
                    addTweens.Add(tic.ScaleTo(1));
                }
                
                if (lockTickOrientation.enabled)
                    lockTickOrientation.value.ApplyTo(tic.latex);
            }

            var removeTweens = ticsToRemove
                .Select(x => x.GetComponent<AxisTic>())
                .OrderByDescending(x => Mathf.Abs(x.value))
                .Select(
                    tick => {
                        updateTweens.Add(tick.MoveTo(GetPosition(tick)));
                        return tick.ScaleTo(0);
                    }
                );
            
            return (
                addTweens.RunInParallel(),
                updateTweens.RunInParallel(),
                removeTweens.RunInParallel()
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
