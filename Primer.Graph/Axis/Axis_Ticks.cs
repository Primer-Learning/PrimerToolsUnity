using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    public partial class Axis
    {
        #region public bool showTicks;
        [SerializeField, HideInInspector]
        private bool _showTicks = true;

        [Title("Ticks")]
        [ShowInInspector]
        public bool showTicks {
            get => _showTicks;
            set {
                _showTicks = value;
                UpdateChildren();
            }
        }
        #endregion

        #region public bool showZero;
        [SerializeField, HideInInspector]
        private bool _showZero;

        [ShowInInspector]
        [EnableIf(nameof(showTicks))]
        public bool showZero {
            get => _showZero;
            set {
                _showZero = value;
                UpdateChildren();
            }
        }
        #endregion

        #region public Optional<Direction> lockTickOrientation;
        [SerializeField, HideInInspector]
        private Optional<Direction> _lockTickOrientation = Direction.Front;

        [ShowInInspector]
        [EnableIf(nameof(showTicks))]
        public Optional<Direction> lockTickOrientation {
            get => _lockTickOrientation;
            set {
                _lockTickOrientation = value;
                UpdateChildren();
            }
        }
        #endregion

        #region public float step;
        [SerializeField, HideInInspector]
        private float _step = 2;

        [ShowInInspector]
        [MinValue(0.1f)]
        [EnableIf(nameof(showTicks))]
        [DisableIf("@manualTicks.Count != 0")]
        public float step {
            get => _step;
            set {
                _step = value;
                UpdateChildren();
            }
        }
        #endregion

        #region public int maxTicks;
        [SerializeField, HideInInspector]
        private int _maxTicks = 50;

        [ShowInInspector]
        [PropertyRange(1, 100)]
        [EnableIf(nameof(showTicks))]
        public int maxTicks {
            get => _maxTicks;
            set {
                _maxTicks = value;
                UpdateChildren();
            }
        }
        #endregion

        #region public int maxDecimals;
        [SerializeField, HideInInspector]
        private int _maxDecimals = 2;

        [ShowInInspector]
        [PropertyRange(0, 10)]
        [EnableIf(nameof(showTicks))]
        public int maxDecimals {
            get => _maxDecimals;
            set {
                _maxDecimals = value;
                UpdateChildren();
            }
        }
        #endregion

        #region public float tickOffset;
        [SerializeField, HideInInspector]
        private float _tickOffset;

        [ShowInInspector]
        [EnableIf(nameof(showTicks))]
        public float tickOffset {
            get => _tickOffset;
            set {
                _tickOffset = value;
                UpdateChildren();
            }
        }
        #endregion

        #region public int labelNumberOffset;
        [SerializeField, HideInInspector]
        private int _labelNumberOffset;

        [ShowInInspector]
        [EnableIf(nameof(showTicks))]
        public int labelNumberOffset {
            get => _labelNumberOffset;
            set {
                _labelNumberOffset = value;
                UpdateChildren();
            }
        }
        #endregion

        #region public float valuePositionOffset;
        [SerializeField, HideInInspector]
        private float _valuePositionOffset;

        [ShowInInspector]
        [EnableIf(nameof(showTicks))]
        public float valuePositionOffset {
            get => _valuePositionOffset;
            set {
                _valuePositionOffset = value;
                UpdateChildren();
            }
        }
        #endregion

        #region public List<TickData> manualTicks;
        [SerializeField, HideInInspector]
        private List<TickData> _manualTicks;

        [ShowInInspector]
        [EnableIf(nameof(showTicks))]
        public List<TickData> manualTicks {
            get => _manualTicks;
            set {
                _manualTicks = value;
                UpdateChildren();
            }
        }
        #endregion

        #region public PrefabProvider<AxisTick> tickPrefab;
        [SerializeField, HideInInspector]
        private PrefabProvider<AxisTick> _tickPrefab;

        [ShowInInspector]
        [RequiredIn(PrefabKind.PrefabAsset)]
        [EnableIf(nameof(showTicks))]
        public PrefabProvider<AxisTick> tickPrefab {
            get => _tickPrefab;
            set {
                _tickPrefab = value;
                UpdateChildren();
            }
        }
        #endregion


        public void UpdateTicks(Container parentContainer)
        {
            if (!showTicks || step <= 0 || tickPrefab.isEmpty)
                return;

            var domain = this;
            var container = parentContainer
                .AddContainer("Ticks container")
                //.ScaleGrandchildrenInPlayMode()
                .SetDefaults();

            var expectedTicks = manualTicks.Count != 0
                ? manualTicks
                : CalculateTics();

            foreach (var data in CropTicksCount(expectedTicks)) {
                var tick = container.Add(tickPrefab, $"Tick {data.label}");
                tick.value = data.value;
                tick.label = data.label;
                tick.transform.localPosition = new Vector3((data.value + valuePositionOffset) * domain.scale, tickOffset, 0);

                if (lockTickOrientation.enabled)
                    lockTickOrientation.value.ApplyTo(tick.latex);
            }

            foreach (var removing in container.removing) {
                var data = removing.GetComponent<AxisTick>();
                removing.localPosition = new Vector3((data.value + valuePositionOffset) * domain.scale, tickOffset, 0);
            }

            container.Purge();
        }

        private List<TickData> CropTicksCount(List<TickData> ticks)
        {
            if (maxTicks <= 0 || ticks.Count <= maxTicks)
                return ticks;

            var pickIndexes = ticks.Count / maxTicks + 1;

            return ticks
                .Where((_, i) => i % pickIndexes == 0)
                .Take(maxTicks - 1)
                .Append(ticks.Last())
                .ToList();
        }

        private List<TickData> CalculateTics()
        {
            var domain = this;
            var calculated = new List<TickData>();
            var multiplier = Mathf.Pow(10, maxDecimals);
            var roundedStep = Mathf.Round(step * multiplier) / multiplier;

            if (roundedStep <= 0)
                return calculated;

            if (showZero)
                calculated.Add(new TickData(0, labelNumberOffset));

            for (var i = Mathf.Max(roundedStep, domain.min); i <= domain.max; i += roundedStep)
                calculated.Add(new TickData(i, labelNumberOffset));

            for (var i = Mathf.Min(-roundedStep, domain.max); i >= domain.min; i -= roundedStep)
                calculated.Add(new TickData(i, labelNumberOffset));

            return calculated;
        }

        [Serializable]
        public class TickData
        {
            public float value;
            public string label;

            public TickData(float value, int labelOffset) {
                this.value = value;
                label = (value + labelOffset).FormatNumberWithDecimals();
            }
        }
    }
}
