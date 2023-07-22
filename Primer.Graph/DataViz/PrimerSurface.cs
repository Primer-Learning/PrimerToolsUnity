using System;
using System.Collections.Generic;
using System.Linq;
using Primer.Animation;
using Primer.Shapes;
using UnityEngine;

namespace Primer.Graph
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    [RequireComponent(typeof(GraphDomain))]
    public class PrimerSurface : MonoBehaviour, IDisposable
    {
        private IGrid renderedGrid = new DiscreteGrid(0);
        private IGrid incomingGrid = null;

        private MeshFilter meshFilterCache;
        private MeshFilter meshFilter => transform.GetOrAddComponent(ref meshFilterCache);

        private MeshRenderer meshRendererCache;
        private MeshRenderer meshRenderer => transform.GetOrAddComponent(ref meshRendererCache);

        private GraphDomain domainCache;
        private GraphDomain domain => transform.GetOrAddComponent(ref domainCache);

        public void SetData(float[,] data)
        {
            incomingGrid = new DiscreteGrid(data.GetLength(0), data.GetLength(1));

            // create x,y,z points from data
            for (var x = 0; x < data.GetLength(0); x++) {
                for (var y = 0; y < data.GetLength(1); y++) {
                    incomingGrid.points[x, y] = new Vector3(x, y, data[x, y]);
                }
            }
        }

        public void SetData(IEnumerable<IEnumerable<Vector3>> data)
        {
            var array = data.Select(x => x.ToArray()).ToArray();

            if (array.Select(x => x.Length).Distinct().Count() != 1)
                throw new Exception("All rows must have the same length");

            var points = new Vector3[array.Length, array[0].Length];

            for (var x = 0; x < array.Length; x++) {
                for (var y = 0; y < array[0].Length; y++) {
                    points[x, y] = array[x][y];
                }
            }

            incomingGrid = new DiscreteGrid(points);
        }

        public void SetData(Vector3[,] data)
        {
            incomingGrid = new DiscreteGrid(data);
        }

        public void SetFunction(Func<float, float, float> function, int? resolution = null)
        {
            throw new NotImplementedException();
        }

        public void SetFunction(Func<Vector2, Vector3> function, int? resolution = null)
        {
            throw new NotImplementedException();
        }

        public Tween Transition()
        {
            if (incomingGrid is null)
                return Tween.noop;

            var targetGrid = incomingGrid;
            var (from, to) = IGrid.SameResolution(renderedGrid, targetGrid);

            incomingGrid = null;

            return new Tween(
                t => Render(IGrid.Lerp(from, to, t))
            ).Observe(
                // After the transition is complete, we ensure we store the line we got
                // instead of the result of IGrid.Lerp() which is always a DiscreteGrid.
                onComplete: () => Render(targetGrid)
            );
        }

        public Tween GrowFromStart()
        {
            var targetGrid = incomingGrid ?? renderedGrid;
            var resolution = targetGrid.resolution;

            return new Tween(
                t => {
                    var cutLength = new Vector2(resolution.x * t, resolution.y * t);
                    Render(targetGrid.SmoothCut(cutLength, fromOrigin: false));
                }
            ).Observe(
                // After the transition is complete, we ensure we store the line we got
                // instead of the result of SmoothCut() which is always a DiscreteGrid.
                onComplete: () => Render(targetGrid)
            );
        }

        public Tween ShrinkToEnd()
        {
            var targetGrid = renderedGrid;
            var resolution = targetGrid.resolution;

            return new Tween(
                t => {
                    var cutLength = new Vector2(resolution.x * t, resolution.y * t);
                    Render(targetGrid.SmoothCut(cutLength, fromOrigin: true));
                }
            );
        }

        public void Dispose()
        {
            new Container(transform).Dispose();
        }

        private void Render(IGrid grid)
        {
            if (grid is null)
                return;

            meshFilter.mesh = new Mesh {
                vertices = DefinePoints(grid),
                triangles = DefineTriangles(grid.resolution),
            };
        }

        private static Vector3[] DefinePoints(IGrid grid)
        {
            var points2D = grid.points;
            var colCount = points2D.GetLength(0);
            var rowCount = points2D.GetLength(1);

            var points = new Vector3[rowCount * colCount];

            for (var y = 0; y < rowCount; y++) {
                for (var x = 0; x < colCount; x++) {
                    points[y * colCount + x] = points2D[x, y];
                }
            }

            return points;
        }

        private static int[] DefineTriangles(Vector2Int size) {
            var triangles = new List<int>();
            var cols = size.x;

            // setting each square's triangles
            for (var x = 0; x < size.x - 1; x++) {
                for (var y = 0; y < size.y - 1; y++) {
                    var topLeft = y * cols + x;
                    var topRight = y * cols + x + 1;
                    var bottomLeft = (y + 1) * cols + x;
                    var bottomRight = (y + 1) * cols + x + 1;

                    triangles.AddTriangle(topLeft, topRight, bottomRight);
                    triangles.AddTriangle(topLeft, bottomLeft, bottomRight);
                }
            }

            return triangles.ToArray();
        }
    }
}
