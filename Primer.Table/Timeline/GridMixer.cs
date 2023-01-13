using System;
using Primer.Animation;
using Primer.Table;
using Primer.Timeline;
using UnityEngine;

namespace Primer.Grid
{
    internal class GridMixer : PrimerMixer<GridGenerator, CellDisplacerBehaviour>
    {
        private CellDisplacerBehaviour currentState;
        public AnimationCurve curve = IPrimerAnimation.cubic;


        protected override void Stop() => trackTarget.DisplaceCells(null);


        protected override IMixerCollector<CellDisplacerBehaviour> CreateCollector() =>
            new MixerCollector<CellDisplacerBehaviour>();

        protected override void Frame(IMixerCollector<CellDisplacerBehaviour> collector)
        {
            var displacer = collector.count == 1
                ? SingleState(collector[0].input, collector[0].weight)
                : Mix(collector);

            trackTarget.DisplaceCells(displacer);
        }

        private static Func<Vector3, float, float, Vector3> SingleState(CellDisplacerBehaviour state, float weight)
        {
            return (vec, tx, ty) => Vector3.Lerp(vec, state.Evaluate(vec, tx, ty), weight);
        }

        private static Func<Vector3, float, float, Vector3> Mix(IMixerCollector<CellDisplacerBehaviour> state)
        {
            return (vec, tx, ty) => {
                var result = vec;

                for (var i = 0; i < state.count; i++) {
                    var (weight, input) = state[i];
                    result = Vector3.Lerp(result, input.Evaluate(vec, tx, ty), weight);
                }

                return result;
            };
        }
    }
}
