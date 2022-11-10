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
                CreateContinuousGrid(1, 0);
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
            var grids = new List<(float weight, Vector3[] points)>();
            var totalWeight = 0f;
            var size = lastSize;

            for (var i = 0; i < count; i++) {
                var weight = playable.GetInputWeight(i);
                if (weight == 0) continue;

                var inputPlayable = (ScriptPlayable<PrimerPlayable>)playable.GetInput(i);
                if (inputPlayable.GetBehaviour() is not ISurfaceBehaviour behaviour) {
                    continue;
                }

                var grid = behaviour?.Points;
                if (grid is null) continue;

                grids.Add((weight, grid));
                totalWeight += weight;
            }

            if (totalWeight == 0) {
                Stop(meshFilter);
                return;
            }

            if (totalWeight < 1) {
                if (grids.Count == 1 && originalMesh.vertices.Length == 0) {
                    ManipulateSingleGrid(grids, out size);
                }
                else {
                    grids.Add((1 - totalWeight, originalMesh.vertices));
                }
            }

            var finalPoints = grids.Count == 1
                ? GridHelper.EnsureIsGrid(grids[0].points, out size)
                : MixGrids(grids, out size);

            SetVertices(finalPoints, size);
        }

        void CreateContinuousGrid(int gridSize, float cellSize) =>
            SetVertices(GridHelper.CreateContinuousGridVectors(gridSize, cellSize), gridSize);

        void SetVertices(Vector3[] points, int newSize) {
            mesh.Clear();
            mesh.vertices = points;
            mesh.triangles = GridHelper.CreateContinuousGridTriangles(newSize);
            mesh.RecalculateNormals();
            lastSize = newSize;
        }

        static Vector3[] NormalizeSize(Vector3[] points, int expectedSize) =>
            GridHelper.ResizeGrid(points, expectedSize);

        static void ManipulateSingleGrid(IList<(float weight, Vector3[] points)> gridsToMix, out int finalSize) {
            var (weight, points) = gridsToMix[0];
            var onlyGrid = GridHelper.EnsureIsGrid(points, out var size);
            var cropAt = size * weight;

            finalSize = Mathf.CeilToInt(cropAt);
            gridsToMix.Clear();
            gridsToMix.Add((1, GridHelper.CropGrid(onlyGrid, size, cropAt)));
        }

        static Vector3[] MixGrids(IReadOnlyList<(float weight, Vector3[] points)> gridsToMix, out int finalSize) {
            var grids = gridsToMix.Count;
            var weights = new float[grids];
            var gridSizes = new int[grids];
            var vertices = new Vector3[grids][];
            var maxSize = 0;

            for (var i = 0; i < grids; i++) {
                weights[i] = gridsToMix[i].weight;
                vertices[i] = GridHelper.EnsureIsGrid(gridsToMix[i].points, out var gridSize, minSize: 1);
                gridSizes[i] = gridSize;
                if (gridSize > maxSize) maxSize = gridSize;
            }

            for (var i = 0; i < grids; i++) {
                if (gridSizes[i] == maxSize) {
                    vertices[i] = NormalizeSize(vertices[i], maxSize);
                }
            }

            var finalPoints = new Vector3[maxSize];

            for (var i = 0; i < maxSize; i++) {
                var point = vertices[0][i];

                for (var j = 1; j < grids; j++) {
                    point = Vector3.Lerp(point, vertices[j][i], weights[j]);
                }

                finalPoints[i] = point;
            }

            finalSize = maxSize;
            return finalPoints;
        }
    }
}
