using System;
using System.Collections.Generic;
using System.Linq;
using Primer.Animation;
using Primer.Shapes;
using UnityEngine;

namespace Primer.Graph
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class PrimerLine : MonoBehaviour
    {
        private ILine renderedLine = new DiscreteLine();
        private ILine incomingLine = null;

        private LineRenderer x;

        private MeshFilter meshFilterCache;
        private MeshFilter meshFilter => transform.GetOrAddComponent(ref meshFilterCache);

        public float width = 1;

        #region SetData(float[] | Vector2[] | Vector3[])
        public void SetData(IEnumerable<float> data)
        {
            var points = data.Select((y, i) => new Vector3(i, y, 0));
            incomingLine = new DiscreteLine(points);
        }

        public void SetData(IEnumerable<Vector2> data)
        {
            incomingLine = new DiscreteLine(data.Cast<Vector3>());
        }

        public void SetData(IEnumerable<Vector3> data)
        {
            incomingLine = new DiscreteLine(data);
        }
        #endregion

        #region AddPoint(float | Vector2 | Vector3)
        public void AddPoint(float data)
        {
            var addTo = GetCurrentDiscreteLine();
            incomingLine = GetCurrentDiscreteLine().Append(new Vector3(addTo.points.Length, data, 0));
        }

        public void AddPoint(Vector2 data)
        {
            incomingLine = GetCurrentDiscreteLine().Append(data);
        }

        public void AddPoint(Vector3 data)
        {
            incomingLine = GetCurrentDiscreteLine().Append(data);
        }
        #endregion

        #region SetFunction(Func<float, float> | Func<float, Vector2> | Func<float, Vector3>, int?, float?, float?)
        public void SetFunction(Func<float, float> function, int? resolution = null, float? xStart = null, float? xEnd = null)
        {
            var current = GetFunctionLineParams();

            incomingLine = new FunctionLine(function) {
                resolution = resolution ?? current.resolution,
                xStart = xStart ?? current.xStart,
                xEnd = xEnd ?? current.xEnd,
            };
        }

        public void SetFunction(Func<float, Vector2> function, int? resolution = null, float? xStart = null, float? xEnd = null)
        {
            var current = GetFunctionLineParams();

            incomingLine = new FunctionLine(function) {
                resolution = resolution ?? current.resolution,
                xStart = xStart ?? current.xStart,
                xEnd = xEnd ?? current.xEnd,
            };
        }

        public void SetFunction(Func<float, Vector3> function, int? resolution = null, float? xStart = null, float? xEnd = null)
        {
            var current = GetFunctionLineParams();

            incomingLine = new FunctionLine(function) {
                resolution = resolution ?? current.resolution,
                xStart = xStart ?? current.xStart,
                xEnd = xEnd ?? current.xEnd,
            };
        }
        #endregion

        public Tween Transition()
        {
            var sameSize = ILine.Resize(renderedLine, incomingLine);
            var from = sameSize[0];
            var to = sameSize[1];

            incomingLine = null;

            return new Tween(t => Render(ILine.Lerp(from, to, t)));
        }

        public Tween GrowFromStart()
        {
            var targetLine = incomingLine ?? renderedLine;
            var resolution = targetLine.resolution;
            return new Tween(t => Render(targetLine.SmoothCut(resolution * t, fromOrigin: false)));
        }

        public Tween ShrinkToEnd()
        {
            var targetLine = renderedLine;
            var resolution = targetLine.resolution;
            return new Tween(t => Render(targetLine.SmoothCut(resolution * t, fromOrigin: true)));
        }

        private void Render(ILine line)
        {
            if (line == null)
                return;


            var points = line.points;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            for (var i = 0; i < points.Length - 1; i++) {
                var currentPoint = points[i];
                var nextPoint = points[i + 1];
                var direction = (nextPoint - currentPoint).normalized;
                var perpendicular = new Vector3(-direction.y, direction.x, 0) * width / 2;

                // Add the four vertices of the rectangle to the list
                vertices.Add(currentPoint + perpendicular);
                vertices.Add(currentPoint - perpendicular);
                vertices.Add(nextPoint + perpendicular);
                vertices.Add(nextPoint - perpendicular);

                // Add the indices of the two triangles to the list
                var index = i * 4;
                triangles.Add(index);
                triangles.Add(index + 1);
                triangles.Add(index + 2);
                triangles.Add(index + 1);
                triangles.Add(index + 3);
                triangles.Add(index + 2);
                triangles.Add(index);
                triangles.Add(index + 2);
                triangles.Add(index + 1);
                triangles.Add(index + 1);
                triangles.Add(index + 2);
                triangles.Add(index + 3);
            }

            meshFilter.mesh = new Mesh {
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray(),
            };

            renderedLine = line;
        }

        private DiscreteLine GetCurrentDiscreteLine()
        {
            var current = incomingLine ?? renderedLine;

            if (current is not DiscreteLine discrete)
                throw new Exception("Cannot add point to non-discrete line");

            return discrete;
        }

        private FunctionLine GetFunctionLineParams()
        {
            if (incomingLine is FunctionLine incoming)
                return incoming;

            if (renderedLine is FunctionLine rendered)
                return rendered;

            return new FunctionLine();
        }
    }
}
