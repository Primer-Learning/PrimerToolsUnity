using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Axis
{
    public class AxisTicks
    {
        private readonly AxisRenderer axis;

        public AxisTicks(AxisRenderer axis) => this.axis = axis;

        public void Update(ChildrenModifier modifier)
        {
            if (!axis.showTics || axis.ticStep <= 0)
                return;

            var expectedTicks = axis.manualTics.Count != 0
                ? axis.manualTics
                : CalculateTics();

            if ((axis.maxTics > 0) && (expectedTicks.Count > axis.maxTics)) {
                // TODO: reduce amount of tics in a smart way
                expectedTicks = expectedTicks.Take(axis.maxTics).ToList();
            }

            foreach (var data in expectedTicks) {
                var tick = modifier.Next()
                    .Called($"Tick {data.label}")
                    .InstantiatedFrom(axis.tickPrefab);

                tick.label = data.label;
                tick.transform.localPosition = new Vector3(data.value * axis.positionMultiplier, 0, 0);
            }
        }

        private List<TicData> CalculateTics()
        {
            var calculated = new List<TicData>();
            var step = Mathf.Round(axis.ticStep * 100) / 100;

            if (step <= 0)
                return calculated;

            for (var i = step; i <= axis.max; i += step)
                calculated.Add(new TicData(i));

            for (var i = -step; i >= axis.min; i -= step)
                calculated.Add(new TicData(i));

            return calculated;
        }
    }
}
