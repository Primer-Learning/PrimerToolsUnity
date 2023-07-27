using System;
using System.Collections.Generic;
using System.Linq;
using Primer.Animation;
using Primer.Shapes;
using UnityEngine;

namespace Primer.Graph
{
    public class StackedArea : MonoBehaviour
    {
        public List<ILine> renderedData = new();
        public List<ILine> incomingData;

        public List<Color> colors = new List<Color>(PrimerColor.all);

        private GraphDomain domainCache;
        private GraphDomain domain => transform.GetOrAddComponent(ref domainCache);

        public void SetData(params IEnumerable<float>[] data)
        {
            incomingData = new List<ILine>(data.Length);

            for (var i = 0; i < data.Length; i++) {
                incomingData.Add(new DiscreteLine(data[i].ToArray()));
            }
        }

        public void AddData(params float[] newColumn)
        {
            incomingData = new List<ILine>(incomingData ?? renderedData);

            for (var y = 0; y < newColumn.Length; y++) {
                incomingData[y] = incomingData[y].ToDiscrete().Append(newColumn[y]);
            }
        }

        public void AddArea(params float[] newArea)
        {
            incomingData = new List<ILine>(incomingData ?? renderedData) {
                new DiscreteLine(newArea),
            };
        }

        public Tween Transition()
        {
            if (incomingData is null)
                return Tween.noop;

            var targetData = incomingData;
            var linesCount = Math.Max(renderedData.Count, targetData.Count);
            var lines = new List<(ILine from, ILine to)>(linesCount);

            for (var i = 0; i < linesCount; i++) {
                var from = i < renderedData.Count ? renderedData[i] : FlatLine(renderedData[0]);
                var to = i < targetData.Count ? targetData[i] : FlatLine(from);
                lines.Add((from, to));
            }

            incomingData = null;

            return new Tween(
                t => Render(lines.Select(x => ILine.Lerp(x.from, x.to, t)))
            ).Observe(
                // After the transition is complete, we ensure we store the line we got
                // instead of the result of ILine.Lerp() which is always a DiscreteLine.
                onComplete: () => Render(targetData)
            );
        }

        public Tween GrowFromStart()
        {
            var targetData = incomingData ?? renderedData;

            return new Tween(
                t => Render(targetData.Select(x => x.SmoothCut(x.resolution * t, fromOrigin: false)))
            ).Observe(
                // After the transition is complete, we ensure we store the line we got
                // instead of the result of SmoothCut() which is always a DiscreteLine.
                onComplete: () => Render(targetData)
            );
        }

        public Tween ShrinkToEnd()
        {
            var targetData = renderedData;

            return new Tween(
                t => Render(targetData.Select(x => x.SmoothCut(x.resolution * (1 - t), fromOrigin: true)))
            ).Observe(
                onComplete: () => gameObject.SetActive(false)
            );
        }

        private void Render(IEnumerable<ILine> data)
        {
            var container = new Gnome(transform);
            var lines = (data ?? new List<ILine>()).ToList();

            if (lines.Count is 0) {
                container.Purge();
                return;
            }

            var prevLine = FlatLine(lines[0])
                .points
                .Select(domain.TransformPoint)
                .ToArray();

            for (var i = 0; i < lines.Count; i++) {
                var points = new List<Vector3>(prevLine);
                var triangles = new List<int>();
                var line = lines[i].points
                    .Select((vec, j) => new Vector3(vec.x, vec.y + prevLine[j].y, vec.z))
                    .Select(domain.TransformPoint)
                    .ToArray();

                var lineStart = points.Count;
                points.Add(line[0]);

                for (var j = 1; j < line.Length; j++) {
                    points.Add(line[j]);

                    var a = j - 1;
                    var b = j;
                    var c = lineStart + j - 1;
                    var d = lineStart + j;

                    triangles.AddTriangle(a, b, d);
                    triangles.AddTriangle(a, c, d);
                }

                var area = container.Add<MeshRenderer>($"Area {i}");
                area.material = MeshRendererExtensions.defaultMaterial;
                area.SetColor(colors[i]);

                area.GetOrAddComponent<MeshFilter>().mesh = new Mesh {
                    vertices = points.ToArray(),
                    triangles = triangles.ToArray(),
                };

                prevLine = line;
            }

            renderedData = lines;
            container.Purge();
        }

        private static DiscreteLine FlatLine(ILine sample)
        {
            var points = sample.points
                .Select(vec => new Vector3(vec.x, 0))
                .ToArray();

            return new DiscreteLine(points);
        }
    }
}
