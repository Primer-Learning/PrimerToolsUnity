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
            playable is ISurfaceBehaviour {Points: {}} behaviour
                ? new ContinuousGrid(behaviour.Points)
                : null;

        protected override IGrid SingleInput(IGrid grid, float weight, bool isReverse) =>
            grid.Crop(grid.Size * weight);

        protected override void Apply(MeshFilter trackTarget, IGrid input) =>
            input.RenderTo(mesh, true);

        protected override IGrid Mix(List<float> weights, List<IGrid> inputs) {
            // IGrid.Lerp is going to resize the grids
            // But we calculate max size in advance so grids
            // only suffer a single transformation

            var maxSize = 0;

            for (var i = 0; i < inputs.Count; i++) {
                var size = inputs[i].Size;
                if (size > maxSize) maxSize = size;
            }

            var result = inputs[0].Resize(maxSize);

            for (var i = 1; i < inputs.Count; i++) {
                result = ContinuousGrid.Lerp(result, inputs[i], weights[i]);
            }

            return result;
        }
    }
}
