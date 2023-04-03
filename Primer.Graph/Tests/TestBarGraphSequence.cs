using System.Collections.Generic;
using Primer.Animation;
using Primer.Shapes;
using Primer.Timeline;
using UnityEngine;

namespace Primer.Graph.Tests
{
    public class TestBarGraphSequence : Sequence
    {
        private readonly IPool<PrimerLine> linePool = PrimerLine.pool
            .ForTimeline()
            .Specialize(x => x.DashedAnimated());

        public BarPlot barPlot;

        public override void Cleanup()
        {
            base.Cleanup();

            linePool.RecycleAll();

            if (barPlot is null)
                return;

            barPlot.SetDefaults();
            barPlot.Clear();
            barPlot.SetNames("First", "Second", "Third");
            barPlot.SetColors(Color.green, Color.blue, Color.magenta);
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

            yield return barPlot[0].Tween(x=>x.color, current => current * 0.5f);

            var line = barPlot.VerticalLine(linePool);
            line.Set(x: barPlot.GetPointBefore("First"));
            yield return line.MoveTo(x: barPlot.GetPointAfter("First"));

            yield return barPlot.Tween(x => x.offset, Vector3.one);
        }
    }
}
