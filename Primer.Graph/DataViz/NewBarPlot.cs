using System;
using System.Collections.Generic;
using System.Linq;
using Primer.Animation;
using Primer.Shapes;
using Sirenix.OdinInspector;
using UnityEngine;

using BarPlotData = System.Collections.Generic.List<System.Collections.Generic.List<float>>;

namespace Primer.Graph
{
    public class NewBarPlot : MonoBehaviour, IDisposable
    {
        public BarPlotData renderedData = new BarPlotData();
        public BarPlotData incomingData;

        public List<Color> colors = new List<Color>(PrimerColor.all);

        private GraphDomain domainCache;
        private GraphDomain domain => transform.GetOrAddComponent(ref domainCache);

        #region public float barGap;
        [SerializeField, HideInInspector]
        private Vector2 _barGap = new Vector2(1, 0.5f);

        [ShowInInspector]
        public Vector2 barGap {
            get => _barGap;
            set {
                _barGap = value;
                Render();
            }
        }
        #endregion

        #region public float offset;
        [SerializeField, HideInInspector]
        private float _offset = 1;

        [ShowInInspector]
        public float offset {
            get => _offset;
            set {
                _offset = value;
                Render();
            }
        }
        #endregion

        #region public float barWidth;
        [SerializeField, HideInInspector]
        private float _barWidth = 1;

        [ShowInInspector]
        public float barWidth {
            get => _barWidth;
            set {
                _barWidth = value;
                Render();
            }
        }
        #endregion

        private void OnEnable()
        {
            domain.behaviour = GraphDomain.Behaviour.InvokeMethod;
            domain.onDomainChange = Render;
        }

        public void SetData(params float[] bars)
        {
            incomingData = ToLists(bars.Select(x => new List<float> { x }));
        }

        public void SetData(float[,] data)
        {
            incomingData = ToLists(data);
        }

        public void SetData(params IEnumerable<float>[] data)
        {
            incomingData = ToLists(data);
        }

        public void AddStack(params float[] data)
        {
            // We create a copy so we can mutate it
            incomingData = ToLists(incomingData ?? renderedData ?? new BarPlotData());

            for (var i = 0; i < data.Length; i++) {
                if (incomingData.Count <= i)
                    incomingData.Add(new List<float>());

                incomingData[i].Add(data[i]);
            }
        }

        public void AddData(params float[] data)
        {
            incomingData = ToLists(incomingData ?? renderedData);
            incomingData.Add(new List<float>(data));
        }

        public Tween Transition()
        {
            if (incomingData is null)
                return Tween.noop;

            var initial = ToLists(renderedData);
            var target = ToLists(incomingData);

            incomingData = null;
            return new Tween(t => Render(Lerp(initial, target, t)));
        }

        public Tween GrowFromStart()
        {
            var initial = new BarPlotData();
            var target = ToLists(incomingData ?? renderedData);
            return new Tween(t => Render(Lerp(initial, target, t)));
        }

        public Tween ShrinkToEnd()
        {
            var initial = renderedData;
            var target = new BarPlotData();
            return new Tween(t => Render(Lerp(initial, target, t)));
        }

        public void Dispose()
        {
            Gnome.Dispose(this);
        }

        private void Render() => Render(renderedData);
        private void Render(BarPlotData data)
        {
            var gnome = Gnome.From(this);
            var width = domain.TransformPoint(new Vector3(barWidth, 0)).x;
            var gap = domain.TransformPoint(barGap);
            var offset = domain.TransformPoint(new Vector3(this.offset, 0)).x;

            for (var x = 0; x < data.Count; x++) {
                var bar = data[x];
                var bottom = domain.TransformPoint(new Vector3(x, 0));
                var coords = bar.Select(y => domain.TransformPoint(new Vector3(x, y))).ToList();

                for (var j = 0; j < data[x].Count; j++) {
                    var rect = gnome.Add<Rectangle>($"Bar {x} stack {j}");
                    rect.transform.localPosition = new Vector3(bottom.x + (gap.x * x) + offset, bottom.y);
                    rect.pivot = RectPivot.BottomCenter;
                    rect.width = width;
                    rect.height = coords[j].y;
                    rect.color = colors[j % colors.Count];
                    bottom = new Vector3(bottom.x, bottom.y + coords[j].y + gap.y);
                }
            }

            renderedData = data;
        }

        private static BarPlotData ToLists(IEnumerable<IEnumerable<float>> data)
        {
            return data.Select(x => x.ToList()).ToList();
        }

        private static BarPlotData ToLists(float[,] data)
        {
            var lists = new List<List<float>>();

            for (var x = 0; x < data.GetLength(0); x++) {
                var list = new List<float>();

                for (var y = 0; y < data.GetLength(1); y++)
                    list.Add(data[x, y]);

                lists.Add(list);
            }

            return lists;
        }

        private static BarPlotData Lerp(BarPlotData a, BarPlotData b, float t)
        {
            var result = new BarPlotData();
            var barsCount = Mathf.Max(a.Count, b.Count);

            for (var i = 0; i < barsCount; i++) {
                var from = i < a.Count ? a[i] : new List<float>();
                var to = i < b.Count ? b[i] : new List<float>();
                var stackCount = Mathf.Max(from.Count, to.Count);
                var lerpedBar = new List<float>();

                for (var j = 0; j < stackCount; j++) {
                    var fromValue = j < from.Count ? from[j] : 0;
                    var toValue = j < to.Count ? to[j] : 0;
                    lerpedBar.Add(Mathf.Lerp(fromValue, toValue, t));
                }

                result.Add(lerpedBar);
            }

            return result;
        }
    }
}
