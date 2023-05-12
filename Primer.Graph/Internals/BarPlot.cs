using System;
using System.Collections.Generic;
using System.Linq;
using Primer.Animation;
using Primer.Shapes;
using Primer.Timeline;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    [RequireComponent(typeof(Graph))]
    public class BarPlot : MonoBehaviour
    {
        public static Color defaultColor = new Vector4(62, 126, 160, 255) / 255;

        private Graph graphCache;
        private Graph graph => graphCache ??= GetComponent<Graph>();


        public BarData this[int index] => GetBar(index);
        public BarData this[string name] => GetBar(name);

        [SerializeField, HideInInspector]
        private float _barWidth = 1f;
        [ShowInInspector]
        public float barWidth {
            get => _barWidth;
            set => Meta.ReactiveProp(ref _barWidth, value, UpdateBars);
        }

        [SerializeField, HideInInspector]
        private float _cornerRadius = 0.25f;
        [ShowInInspector]
        public float cornerRadius {
            get => _cornerRadius;
            set => Meta.ReactiveProp(ref _cornerRadius, value, UpdateBars);
        }

        [SerializeField, HideInInspector]
        private Vector3 _offset = Vector3.zero;
        [ShowInInspector]
        public Vector3 offset {
            get => _offset;
            set => Meta.ReactiveProp(ref _offset, value, UpdateBars);
        }

        [SerializeField, HideInInspector]
        private List<BarData> _bars = new();
        [ShowInInspector]
        [HorizontalGroup("Data")]
        [HideReferenceObjectPicker]
        [ListDrawerSettings(Expanded = true, AlwaysAddDefaultValue = true)]
        private List<BarData> bars {
            get => _bars;
            set => Meta.ReactiveProp(ref _bars, value, UpdateBars);
        }


        public void SetDefaults()
        {
            _barWidth = 0.8f;
            _cornerRadius = 0.25f;
            _offset = Vector3.right * 0.5f;
            _bars.Clear();
        }


        #region Unity events
        public void OnEnable()
        {
            graph.onDomainChanged += UpdateBars;
            UpdateBars();
        }

        public void OnDisable()
        {
            graph.onDomainChanged -= UpdateBars;
        }
        #endregion


        #region Bar management
        public BarData AddBar(string name, float value = 0) => AddBar(name, value, defaultColor);
        public BarData AddBar(string name, float value, Color color)
        {
            var bar = new BarData {
                name = name,
                value = value,
                color = color,
            };

            bars.Add(bar);
            UpdateBars();
            return bar;
        }

        public BarData GetBar(int index, bool createIfMissing = false)
        {
            if (createIfMissing)
                EnsureBarCount(index);
            else if (bars.Count <= index)
                throw new IndexOutOfRangeException($"There is no bar in the graph at index {index}");

            return bars[index];
        }

        public BarData GetBar(string name, bool createIfMissing = false)
        {
            var bar = bars.FirstOrDefault(x => x.name == name);

            if (bar is not null)
                return bar;

            if (createIfMissing)
                return AddBar(name);

            throw new IndexOutOfRangeException($"There is no bar in the graph called {name}");
        }

        public void Clear()
        {
            bars.Clear();
            UpdateBars();
        }

        private T[] EnsureBarCount<T>(IEnumerable<T> input)
        {
            var array = input.ToArray();
            EnsureBarCount(array.Length);
            return array;
        }

        private void EnsureBarCount(int inputCount)
        {
            if (bars.Count == inputCount) return;

            for (var i = bars.Count; i < inputCount; i++)
                bars.Add(new BarData { color = defaultColor });

            UpdateBars();
        }
        #endregion


        #region Set / tween properties
        public void SetNames(IEnumerable<string> names) => SetNames(names.ToArray());
        public void SetNames(params string[] names)
        {
            var input = EnsureBarCount(names);

            for (var i = 0; i < input.Length; i++)
                bars[i].name = input[i];
        }

        public void SetValues(IEnumerable<int> values) => SetValues(values.ToFloatArray());
        public void SetValues(int[] values) => SetValues(values.ToFloatArray());
        public void SetValues(IEnumerable<float> values) => SetValues(values.ToArray());
        public void SetValues(params float[] values)
        {
            var input = EnsureBarCount(values);

            for (var i = 0; i < input.Length; i++)
                bars[i].value = input[i];
        }

        public Tween TweenValues(IEnumerable<int> values) => TweenValues(values.ToFloatArray());
        public Tween TweenValues(params int[] values) => TweenValues(values.ToFloatArray());
        public Tween TweenValues(IEnumerable<float> values) => TweenValues(values.ToArray());
        public Tween TweenValues(params float[] values)
        {
            var to = EnsureBarCount(values);
            var from = to.Select((x, i) => bars[i].value).ToArray();

            return new Tween(t => {
                for (var i = 0; i < to.Length; i++)
                    bars[i].value = Mathf.Lerp(from[i], to[i], t);
            });
        }

        public void SetColors(IEnumerable<Color> colors) => SetColors(colors.ToArray());
        public void SetColors(params Color[] colors)
        {
            var input = EnsureBarCount(colors);

            for (var i = 0; i < input.Length; i++)
                bars[i].color = input[i];
        }

        public Tween TweenColors(IEnumerable<Color> newColors) => TweenColors(newColors.ToArray());
        public Tween TweenColors(params Color[] newColors)
        {
            var to = EnsureBarCount(newColors);
            var from = to.Select((x, i) => bars[i].color).ToArray();

            return new Tween(t => {
                for (var i = 0; i < to.Length; i++)
                    bars[i].color = Color.Lerp(from[i], to[i], t);
            });
        }

        // /* can be done with */ barPlot[index].color = color;
        [Obsolete("This method may not be necessary, if you use it please remove this attribute and the comment above")]
        public void SetBarColor(int index, Color color) => this[index].color = color;

        // /* can be done with */ barPlot[name].color = color;
        [Obsolete("This method may not be necessary, if you use it please remove this attribute and the comment above")]
        public void SetBarColor(string name, Color color) => this[name].color = color;

        // /* can be done with */ barPlot[index].Tween("color", color);
        [Obsolete("This method may not be necessary, if you use it please remove this attribute and the comment above")]
        public void TweenColor(int index, Color color) => this[index].Tween("color", color);

        // /* can be done with */ barPlot[name].Tween("color", color);
        [Obsolete("This method may not be necessary, if you use it please remove this attribute and the comment above")]
        public void TweenColor(string name, Color color) => this[name].Tween("color", color);
        #endregion


        #region Line helpers
        public PrimerLine VerticalLine(IPool<PrimerLine> pool = null)
        {
            var line = (pool ?? PrimerLine.pool).Get();
            line.transform.SetParent(graph.domain, true);
            line.transform.localScale = Vector3.one;
            line.start = Vector3.zero;
            line.end = Vector3.up * graph.enabledYAxis.max;
            return line;
        }

        public float GetPointBefore(int index) => GetPointBefore(GetBar(index));
        public float GetPointBefore(string name) => GetPointBefore(GetBar(name));
        public float GetPointBefore(BarData bar)
        {
            var index = bars.IndexOf(bar);
            return index + offset.x;
        }

        public float GetPointAfter(int index) => GetPointAfter(GetBar(index));
        public float GetPointAfter(string name) => GetPointAfter(GetBar(name));
        public float GetPointAfter(BarData bar)
        {
            var index = bars.IndexOf(bar);
            return index + offset.x + 1;
        }
        #endregion


        // public static Dictionary<float, string> GenerateIntegerCategories(
        //     int num,
        //     int min = 0,
        //     int step = 1
        // ) {}


        [Button]
        public void UpdateBars()
        {
            var container = GetContainer();
            var children = new ChildrenDeclaration(container);

            for (var i = 0; i < bars.Count; i++) {
                var data = bars[i];

                var bar = children.Next<Rectangle>(
                    name: data.name ?? $"Bar {i}",
                    init: x => {
                        x.Type = Rectangle.RectangleType.RoundedSolid;
                        x.CornerRadiusMode = Rectangle.RectangleCornerRadiusMode.PerCorner;
                        x.Pivot = RectPivot.Corner;
                    }
                );

                bar.Color = data.color;
                bar.Width = barWidth;
                bar.Height = data.value;

                bar.transform.localPosition = new Vector3(i  + (1 - barWidth) / 2, 0, 0) + offset;
                bar.transform.localScale = Vector3.one;
                bar.CornerRadii = new Vector4(0, cornerRadius, cornerRadius, 0);

                data.onNameChange = UpdateIfError<string>(newName => bar.gameObject.name = newName);
                data.onColorChange = UpdateIfError<Color>(newColor => bar.Color = newColor);
                data.onValueChange = UpdateIfError<float>(newValue => bar.Height = newValue);
            }

            children.Apply();
        }

        private Action<T> UpdateIfError<T>(Action<T> handler)
        {
            return x => {
                try {
                    handler(x);
                }
                catch {
                    Debug.LogWarning("Error updating bar plot, updating all bars");
                    UpdateBars();
                }
            };
        }

        private Transform barsContainer;
        private Transform GetContainer()
        {
            if (barsContainer == null)
                barsContainer = new GameObject("Plotted bars").transform;

            if (barsContainer.parent != graph.domain)
                barsContainer.parent = graph.domain;

            barsContainer.localScale = Vector3.one;
            barsContainer.localPosition = Vector3.zero;
            PrimerTimeline.MarkAsEphemeral(barsContainer);
            return barsContainer;
        }
    }
}
