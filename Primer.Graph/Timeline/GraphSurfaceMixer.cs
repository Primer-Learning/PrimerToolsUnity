using System.Collections.Generic;
using System.Linq;
using Primer.Timeline;
using UnityEngine;

namespace Primer.Graph
{
    public class GraphSurfaceMixer : PrimerMixer<MeshFilter, IGrid>
    {
        private readonly Mesh mesh = new();
        private Mesh originalMesh;

        protected override void Start()
        {
            if (trackTarget == null)
                return;

            originalMesh = trackTarget.sharedMesh;
            trackTarget.mesh = mesh;
        }

        protected override void Stop()
        {
            if (trackTarget != null)
                trackTarget.mesh = originalMesh;

            originalMesh = null;
        }

        protected override IMixerCollector<IGrid> CreateCollector()
        {
            return new CollectorWithDirection<PrimerPlayable, IGrid>(
                playable => playable is ISurfaceBehaviour { Grid: {} } behaviour
                    ? behaviour.Grid
                    : null
            );
        }

        protected override void Frame(IMixerCollector<IGrid> genericCollector)
        {
            var collector = (CollectorWithDirection<PrimerPlayable, IGrid>)genericCollector;

            var state = collector.count > 1
                ? Mix(collector.weights, collector.inputs)
                : collector.isFull
                    ? collector[0].input
                    : CutGrid(collector[0].input, collector[0].weight, collector.isReverse);

            ApplyState(state);
        }

        protected static IGrid CutGrid(IGrid grid, float weight, bool isReverse) =>
            grid.SmoothCut(grid.Size * weight, isReverse);

        protected static IGrid Mix(IReadOnlyList<float> weights, IReadOnlyList<IGrid> inputs)
        {
            var grids = IGrid.Resize(inputs.ToArray());
            var result = grids[0];

            for (var i = 1; i < inputs.Count; i++)
                result = IGrid.Lerp(result, grids[i], weights[i]);

            return result;
        }

        protected void ApplyState(IGrid input) =>
            input.RenderTo(mesh, true);
    }
}
