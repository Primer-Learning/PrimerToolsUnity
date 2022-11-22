using System.Collections.Generic;
using Primer.Timeline;
using UnityEngine;

namespace Primer.Graph
{
    public class GraphSurfaceMixer : CollectedMixer<MeshFilter, IGrid>
    {
        readonly Mesh mesh = new();
        Mesh originalMesh;

        protected override void Start(MeshFilter meshFilter) {
            if (meshFilter != null) {
                originalMesh = meshFilter.sharedMesh;
                meshFilter.mesh = mesh;
            }

            originalValue = originalMesh && originalMesh.vertices.Length != 0
                ? new ContinuousGrid(originalMesh.vertices)
                : null;
        }

        protected override void Stop(MeshFilter meshFilter) {
            if (meshFilter != null) {
                meshFilter.mesh = originalMesh;
            }

            originalValue = null;
            originalMesh = null;
        }

        protected override IGrid ProcessPlayable(PrimerPlayable playable) =>
            playable is ISurfaceBehaviour {Grid: {}} behaviour
                ? behaviour.Grid
                : null;

        protected override IGrid SingleInput(IGrid grid, float weight, bool isReverse) =>
            grid.SmoothCut(grid.Size * weight, isReverse);

        protected override void Apply(MeshFilter trackTarget, IGrid input) =>
            input.RenderTo(mesh, true);

        protected override IGrid Mix(List<float> weights, List<IGrid> inputs) {
            var grids = IGrid.Resize(inputs.ToArray());
            var result = grids[0];

            for (var i = 1; i < inputs.Count; i++) {
                result = IGrid.Lerp(result, grids[i], weights[i]);
            }

            return result;
        }
    }
}
