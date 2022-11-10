using System.Collections.Generic;
using Primer.Timeline;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Graph
{
    public class GraphSurfaceMixer : PrimerPlayable<MeshFilter>
    {
        readonly Mesh mesh = new();
        bool isMeshInitialized;
        bool hasModifiedMesh;
        Mesh originalMesh;
        int lastSize;

        public override void Start(MeshFilter meshFilter) {
            if (!isMeshInitialized) {
                var grid = ContinuousGrid.zero;
                grid.RenderTo(mesh);
                isMeshInitialized = true;
            }

            originalMesh = meshFilter.sharedMesh;
            meshFilter.mesh = mesh;
            hasModifiedMesh = true;
        }

        public override void Stop(MeshFilter meshFilter) {
            if (!hasModifiedMesh || meshFilter == null) return;
            meshFilter.mesh = originalMesh;
            originalMesh = null;
            hasModifiedMesh = false;
        }

        public override void Frame(MeshFilter meshFilter, Playable playable, FrameData info) {
            var count = playable.GetInputCount();
            var grids = new List<(float weight, IGrid grid)>();
            var totalWeight = 0f;

            for (var i = 0; i < count; i++) {
                var weight = playable.GetInputWeight(i);
                if (weight == 0) continue;

                var inputPlayable = (ScriptPlayable<PrimerPlayable>)playable.GetInput(i);
                if (inputPlayable.GetBehaviour() is not ISurfaceBehaviour behaviour) {
                    continue;
                }

                var grid = behaviour.Points;
                if (grid is null) continue;

                grids.Add((weight, new ContinuousGrid(grid)));
                totalWeight += weight;
            }

            if (totalWeight == 0) {
                Stop(meshFilter);
                return;
            }

            if (totalWeight < 1) {
                if (grids.Count == 1) {
                    ManipulateSingleGrid(grids);
                }
                else {
                    grids.Add((1 - totalWeight, new ContinuousGrid(originalMesh.vertices)));
                }
            }

            var finalGrid = grids.Count == 1
                ? grids[0].grid
                : MixGrids(grids);

            finalGrid.RenderTo(mesh, true);
        }

        static void ManipulateSingleGrid(IList<(float, IGrid)> gridsToMix) {
            var (weight, onlyGrid) = gridsToMix[0];
            gridsToMix.Clear();
            gridsToMix.Add((1, onlyGrid.Crop(onlyGrid.Size * weight)));
        }

        static IGrid MixGrids(IReadOnlyList<(float weight, IGrid grid)> gridsToMix) {
            var layers = gridsToMix.Count;
            var weights = new float[layers];
            var grids = new IGrid[layers];
            var maxSize = 0;

            for (var i = 0; i < layers; i++) {
                weights[i] = gridsToMix[i].weight;
                var grid = grids[i] = gridsToMix[i].grid;
                if (grid.Size > maxSize) maxSize = grid.Size;
            }

            for (var i = 0; i < layers; i++) {
                if (grids[i].Size != maxSize) {
                    grids[i] = grids[i].Resize(maxSize);
                }
            }

            var finalPoints = new Vector3[maxSize];

            for (var i = 0; i < maxSize; i++) {
                var point = grids[0].Points[i];

                for (var j = 1; j < layers; j++) {
                    point = Vector3.Lerp(point, grids[j].Points[i], weights[j]);
                }

                finalPoints[i] = point;
            }

            return new ContinuousGrid(finalPoints);
        }
    }
}
