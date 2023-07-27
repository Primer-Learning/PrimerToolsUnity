using System;
using System.Collections.Generic;
using System.Linq;
using Primer.Animation;
using Primer.Shapes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    [RequireComponent(typeof(GraphDomain))]
    public class PrimerLine : MonoBehaviour, IMeshController, IDisposable
    {
        private ILine renderedLine = new DiscreteLine(0);
        private ILine incomingLine = null;

        private MeshFilter meshFilterCache;
        private MeshFilter meshFilter => transform.GetOrAddComponent(ref meshFilterCache);

        private MeshRenderer meshRendererCache;
        private MeshRenderer meshRenderer => transform.GetOrAddComponent(ref meshRendererCache);

        private GraphDomain domainCache;
        private GraphDomain domain => transform.GetOrAddComponent(ref domainCache);

        #region public float width;
        [SerializeField, HideInInspector]
        private float _width = 0.2f;

        [ShowInInspector]
        public float width {
            get => _width;
            set {
                _width = value;
                Render();
            }
        }
        #endregion

        #region public float resolution;
        [ShowInInspector]
        [MinValue(1)]
        public int resolution {
            get => renderedLine.resolution;
            set => Render(renderedLine.ChangeResolution(value));
        }
        #endregion

        #region public int endCapVertices;
        [SerializeField, HideInInspector]
        private int _endCapVertices = 18;

        [ShowInInspector]
        public int endCapVertices {
            get => _endCapVertices;
            set {
                _endCapVertices = value;
                Render();
            }
        }
        #endregion

        #region public int miterVertices;
        [SerializeField, HideInInspector]
        private int _miterVertices = 3;

        [ShowInInspector]
        public int miterVertices {
            get => _miterVertices;
            set {
                _miterVertices = value;
                Render();
            }
        }
        #endregion

        private void OnEnable()
        {
            domain.behaviour = GraphDomain.Behaviour.InvokeMethod;
            domain.onDomainChange = Render;
            meshRenderer.sharedMaterial ??= MeshRendererExtensions.defaultMaterial;
        }

        /// <summary>
        ///   Allows to set one lines above the others.
        ///   Positive values will move the line closer to the camera.
        ///   Default index is 0 for all lines.
        /// </summary>
        public void SetZIndex(int index)
        {
            var pos = transform.localPosition;
            transform.localPosition = new Vector3(pos.x, pos.y, index * -0.01f);
        }

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
                start = xStart ?? current.start,
                end = xEnd ?? current.end,
            };
        }

        public void SetFunction(Func<float, Vector2> function, int? resolution = null, float? xStart = null, float? xEnd = null)
        {
            var current = GetFunctionLineParams();

            incomingLine = new FunctionLine(function) {
                resolution = resolution ?? current.resolution,
                start = xStart ?? current.start,
                end = xEnd ?? current.end,
            };
        }

        public void SetFunction(Func<float, Vector3> function, int? resolution = null, float? xStart = null, float? xEnd = null)
        {
            var current = GetFunctionLineParams();

            incomingLine = new FunctionLine(function) {
                resolution = resolution ?? current.resolution,
                start = xStart ?? current.start,
                end = xEnd ?? current.end,
            };
        }
        #endregion

        public Tween Transition()
        {
            if (incomingLine is null)
                return Tween.noop;

            var targetLine = incomingLine;
            var (from, to) = ILine.SameResolution(renderedLine, targetLine);

            incomingLine = null;

            return new Tween(
                t => Render(ILine.Lerp(from, to, t))
            ).Observe(
                // After the transition is complete, we ensure we store the line we got
                // instead of the result of ILine.Lerp() which is always a DiscreteLine.
                onComplete: () => Render(targetLine)
            );
        }

        public Tween GrowFromStart()
        {
            var targetLine = incomingLine ?? renderedLine;
            var resolution = targetLine.resolution;

            return new Tween(
                t => Render(targetLine.SmoothCut(resolution * t, fromOrigin: false))
            ).Observe(
                // After the transition is complete, we ensure we store the line we got
                // instead of the result of SmoothCut() which is always a DiscreteLine.
                onComplete: () => Render(targetLine)
            );
        }

        public Tween ShrinkToEnd()
        {
            var targetLine = renderedLine;
            var resolution = targetLine.resolution;

            return new Tween(
                t => Render(targetLine.SmoothCut(resolution * (1 - t), fromOrigin: true))
            ).Observe(
                onComplete: () => gameObject.SetActive(false)
            );
        }

        public void Dispose()
        {
            Gnome.Dispose(this);
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

            return FunctionLine.Default();
        }

        public MeshRenderer[] GetMeshRenderers() => new[] { meshRenderer };

        private void Render() => Render(renderedLine);
        private void Render(ILine line)
        {
            if (line == null)
                return;

            var points = line.points.Select(domain.TransformPoint).ToArray();
            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            CreateSegments(points, vertices, triangles);
            AddMiter(points, vertices, triangles);

            if (points.Length is not 0 && endCapVertices > 1) {
                if (points.Length is 1) {
                    AddEndCap(vertices, triangles, points[0], Vector3.left);
                    AddEndCap(vertices, triangles, points[0], Vector3.right);
                }
                else {
                    AddEndCap(vertices, triangles, points[0], points[1]);
                    AddEndCap(vertices, triangles, points[^1], points[^2]);
                }
            }

            meshFilter.mesh = new Mesh {
                vertices = vertices.Select(domain.TransformPoint).ToArray(),
                triangles = triangles.ToArray(),
            };

            renderedLine = line;
        }


        #region Drawing mesh
        private void CreateSegments(Vector3[] points, List<Vector3> vertices, List<int> triangles)
        {
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
                triangles.AddTriangle(index, index + 1, index + 2);
                triangles.AddTriangle(index + 1, index + 2, index + 3);
            }
        }

        private void AddMiter(Vector3[] points, List<Vector3> vertices, List<int> triangles)
        {
            // Join segments with triangles in a curve
            for (var i = 0; i < points.Length - 2; i++) {
                var a = points[i];
                var b = points[i + 1];
                var c = points[i + 2];

                var quadIndex = i * 4;
                var center = vertices.Count;
                vertices.Add(b);

                var cross = Vector3.Cross(b - a, c - b);
                var (leftIndex, rightIndex) = cross.z < 0
                    ? (quadIndex + 2, quadIndex + 4)
                    : (quadIndex + 3, quadIndex + 5);

                var left = vertices[leftIndex];
                var right = vertices[rightIndex];

                // miterVertices can also be calculated from the amount of degrees in the angle
                // miterVertices = Vector3.Angle(left - b, right - b) / degreesPerVertex

                if (miterVertices <= 0) {
                    triangles.AddTriangle(center, leftIndex, rightIndex);
                    continue;
                }

                for (var j = 0; j < miterVertices; j++) {
                    var t = (j + 1) / (float)(miterVertices + 1);
                    vertices.Add(Vector3.Slerp(left - b, right - b, t) + b);

                    var corner = j == 0 ? leftIndex : center + j;
                    triangles.AddTriangle(center, corner, center + j + 1);
                }

                triangles.AddTriangle(center, center + miterVertices, rightIndex);
            }
        }

        private void AddEndCap(List<Vector3> vertices, List<int> triangles, Vector3 point, Vector3 prev)
        {
            var direction = -(prev - point).normalized * width / 2;
            var perpendicular = new Vector3(direction.y, -direction.x, 0);
            var center = vertices.Count;
            var totalVertices = (float)endCapVertices + 2;

            vertices.Add(point);
            vertices.Add(point - perpendicular);

            for (var i = 1; i <= totalVertices; i++) {
                var t = i / totalVertices;
                var a = t < 0.5
                    ? Vector3.Slerp(-perpendicular, direction, t * 2)
                    : Vector3.Slerp(direction, perpendicular, t * 2 - 1);

                vertices.Add(a + point);
                triangles.AddTriangle(center, center + i, center + i + 1);
            }
        }
        #endregion
    }
}
