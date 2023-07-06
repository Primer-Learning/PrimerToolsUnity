using System;
using System.Collections.Generic;
using System.Linq;
using Primer.Animation;
using Primer.Latex;
using Primer.Shapes;
using Primer.Timeline;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    [RequireComponent(typeof(Graph))]
    public class BarPlot : MonoBehaviour
    {
        public static Color defaultColor = new Vector4(62, 126, 160, 255) / 255;
        private int barCountForHax;

        private Graph graphCache;
        private Graph graph => transform.GetOrAddComponent(ref graphCache);

        private Container _barsContainer;
        private Container barsContainer => _barsContainer ??= new Container("Plotted bars", graph.domain);

        private Container _labelsContainer;
        public Container labelsContainer
            => _labelsContainer ??= new Container("Plotted bars labels", graph).ScaleChildrenInPlayMode();

        public BarData this[int index] => GetBar(index);
        public BarData this[string name] => GetBar(name);

        public Func<BarData, string> barLabels = null;

        #region float barWidth;
        [SerializeField, HideInInspector]
        private float _barWidth = 1f;
        [ShowInInspector]
        public float barWidth {
            get => _barWidth;
            set => Meta.ReactiveProp(ref _barWidth, value, UpdateBars);
        }
        #endregion

        #region float cornerRadius;
        [SerializeField, HideInInspector]
        private float _cornerRadius = 0.25f;
        [ShowInInspector]
        public float cornerRadius {
            get => _cornerRadius;
            set => Meta.ReactiveProp(ref _cornerRadius, value, UpdateBars);
        }
        #endregion

        #region Vector3 offset;
        [SerializeField, HideInInspector]
        private Vector3 _offset = Vector3.zero;
        [ShowInInspector]
        public Vector3 offset {
            get => _offset;
            set => Meta.ReactiveProp(ref _offset, value, UpdateBars);
        }
        #endregion

        #region Vector3 labelOffset;
        [SerializeField, HideInInspector]
        private Vector3 _labelOffset = Vector3.zero;
        [ShowInInspector]
        public Vector3 labelOffset {
            get => _labelOffset;
            set => Meta.ReactiveProp(ref _labelOffset, value, UpdateBars);
        }
        #endregion

        #region Vector3 labelScale;
        [SerializeField, HideInInspector]
        private Vector3 _labelScale = Vector3.one;
        [ShowInInspector]
        public Vector3 labelScale {
            get => _labelScale;
            set => Meta.ReactiveProp(ref _labelScale, value, UpdateBars);
        }
        #endregion


        #region List<BarData> bars;
        [SerializeField, HideInInspector]
        private List<BarData> _bars = new();
        [ShowInInspector]
        [HorizontalGroup("Data")]
        [HideReferenceObjectPicker]
        [ListDrawerSettings(ShowFoldout = true, AlwaysAddDefaultValue = true)]
        private List<BarData> bars {
            get => _bars;
            set => Meta.ReactiveProp(ref _bars, value, UpdateBars);
        }
        #endregion


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
                for (var i = 0; i < to.Length; i++) {
                    bars[i].value = Mathf.Lerp(from[i], to[i], t);
                }
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
        public void TweenColor(int index, Color color) => Tween.Value(() => this[index].color, color);

        // /* can be done with */ barPlot[name].Tween("color", color);
        [Obsolete("This method may not be necessary, if you use it please remove this attribute and the comment above")]
        public void TweenColor(string name, Color color) => Tween.Value(() => this[name].color, color);
        #endregion


        [Button]
        public void UpdateBars()
        {
            var container = barsContainer;
            container.Reset();
            container.localScale = Vector3.one;
            container.localPosition = Vector3.zero;

            labelsContainer.Reset();

            for (var i = 0; i < bars.Count; i++) {
                var data = bars[i];
                var bar = CreateBar(container, data, i);
                var label = barLabels is null ? null : CreateBarLabel(bar, data, i >= barCountForHax || barCountForHax == 0);

                data.onNameChange = UpdateIfError<string>(newName => bar.gameObject.name = newName);
                data.onColorChange = UpdateIfError<Color>(newColor => bar.color = newColor);
                data.onValueChange = UpdateIfError<float>(newValue => {
                    bar.height = newValue;
                    label?.Process(barLabels(data));
                });
            }

            barCountForHax = bars.Count;
            
            labelsContainer.Purge();
            container.Purge();
        }

        private LatexComponent CreateBarLabel(Rectangle bar, BarData data, bool isNew = false)
        {
            var domain = graph.domain;
            
            var follower = labelsContainer.AddFollower(bar);
            follower.component.useGlobalSpace = true;
            if (isNew)
            {
                follower.localScale = Vector3.zero;
            }

            follower.component.getter = () => {
                if (bar == null) {
                    follower.component.Dispose();
                    return Vector3.zero;
                }

                var barMiddleTop = new Vector3(barWidth / 2, data.value);
                var domainPosition = bar.transform.localPosition + barMiddleTop;
                // return domain.TransformPoint(domainPosition);
                var graphSpace = domain.localScale.ElementWiseMultiply(domainPosition);
                return graph.transform.TransformPoint(graphSpace + labelOffset);
            };

            var label = follower.AddLatex(barLabels(data), $"{bar.name} label");
            label.transform.localPosition = Vector3.zero;
            label.transform.localScale = labelScale;
            // Puts the label in the right place on the first frame, rather than at the origin
            follower.transform.position = follower.component.getter();
            return label;
        }

        private Rectangle CreateBar(Container container, BarData data, int i)
        {
            var bar = container.Add<Rectangle>(data.name ?? $"Bar {i}");

            bar.transform.localPosition = new Vector3(i, 0, 0) + offset;

            bar.pivot = RectPivot.BottomLeft;
            bar.color = data.color;
            bar.width = barWidth;
            bar.height = data.value;

            return bar;
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
    }
}
