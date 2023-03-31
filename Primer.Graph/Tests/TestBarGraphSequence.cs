using System.Collections.Generic;
using Primer.Animation;
using Primer.Timeline;
using UnityEngine;

namespace Primer.Graph.Tests
{
    public class TestBarGraphSequence : Sequence
    {
        public BarPlot barPlot;

        public override void Cleanup()
        {
            base.Cleanup();

            if (barPlot is null)
                return;

            barPlot.SetDefaults();
            barPlot.Clear();
            barPlot.SetNames("First", "Second", "Third");
        }

        public override async IAsyncEnumerator<Tween> Run()
        {
            yield return barPlot.TweenValues(1, 2, 3);

            yield return Parallel(
                barPlot.Tween(x => x.barWidth, 2),
                barPlot.Tween("spacing", 0.5f),
                barPlot.Tween(x => x.cornerRadius, 1)
            );

            // We need to specify the type because it can't be detected from usage
            // The commented line below is equivalent and doesn't need the type because
            // it knows we're accessing "value" field of BarData
            yield return barPlot["Second"].Tween<float>("value", current => current * 2);
            // yield return barPlot["Second"].Tween(x => x.value, current => current * 2);

            yield return barPlot.Tween(x => x.offset, Vector3.one);
        }
    }
}
