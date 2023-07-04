using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    [ExecuteAlways]
    public class MultipleAxesController : MonoBehaviour
    {
        [NonSerialized]
        private Axis[] axes;

        [NonSerialized]
        private Action lastOnDomainChange;

        #region public float scale;
        [SerializeField, HideInInspector]
        private float _scale = 1;

        [ShowInInspector]
        [Title("All axes", "Change properties in all axes at once")]
        [MinValue(0.1f)]
        public float scale {
            get => _scale;
            set {
                _scale = value;
                UpdateAxes(value, x => x.scale);
            }
        }
        #endregion

        #region public Vector2 padding;
        [SerializeField, HideInInspector]
        private Vector2 _padding = Vector2.zero;

        [ShowInInspector]
        private Vector2 padding {
            get => _padding;
            set {
                _padding = value;
                UpdateAxes(value, x => x.padding);
            }
        }
        #endregion

        #region public float rodThickness;
        [SerializeField, HideInInspector]
        private float _rodThickness = 1;

        [ShowInInspector]
        public float rodThickness {
            get => _rodThickness;
            set {
                _rodThickness = value;
                UpdateAxes(value, x => x.thickness);
            }
        }
        #endregion

        #region public ArrowPresence arrowPresence;
        [SerializeField, HideInInspector]
        private ArrowPresence _arrowPresence = ArrowPresence.Both;

        [Title("Arrows")]
        [ShowInInspector]
        public ArrowPresence arrowPresence {
            get => _arrowPresence;
            set {
                _arrowPresence = value;
                UpdateAxes(value, x => x.arrowPresence);
            }
        }
        #endregion

        #region public PrefabProvider arrowPrefab;
        [SerializeField, HideInInspector]
        private PrefabProvider _arrowPrefab = null;

        [ShowInInspector]
        [InlineProperty]
        [RequiredIn(PrefabKind.PrefabAsset)]
        public PrefabProvider arrowPrefab {
            get => _arrowPrefab;
            set {
                _arrowPrefab = value;
                UpdateAxes(value, x => x.arrowPrefab);
            }
        }
        #endregion

        #region public bool showTicks;
        [SerializeField, HideInInspector]
        private bool _showTicks = true;

        [ShowInInspector]
        [Title("Ticks")]
        public bool showTicks {
            get => _showTicks;
            set {
                _showTicks = value;
                UpdateAxes(value, x => x.showTicks);
            }
        }
        #endregion

        #region public bool showZero;
        [SerializeField, HideInInspector]
        private bool _showZero = true;

        [ShowInInspector]
        public bool showZero {
            get => _showZero;
            set {
                _showZero = value;
                UpdateAxes(value, x => x.showZero);
            }
        }
        #endregion

        #region public Optional<Direction> lockTickOrientation;
        [SerializeField, HideInInspector]
        private Optional<Direction> _lockTickOrientation;

        [ShowInInspector]
        [EnableIf(nameof(showTicks))]
        public Optional<Direction> lockTickOrientation {
            get => _lockTickOrientation;
            set {
                _lockTickOrientation = value;
                UpdateAxes(value, x => x.lockTickOrientation);
            }
        }
        #endregion

        #region public float tickSteps;
        [SerializeField, HideInInspector]
        private float _tickSteps = 2;

        [ShowInInspector]
        [MinValue(0.1f)]
        [EnableIf("showTicks")]
        [DisableIf("@manualTicks.Count != 0")]
        public float tickSteps {
            get => _tickSteps;
            set {
                _tickSteps = value;
                UpdateAxes(value, x => x.step);
            }
        }
        #endregion

        #region public int maxTicks;
        [SerializeField, HideInInspector]
        private int _maxTicks = 50;

        [ShowInInspector]
        [PropertyRange(1, 100)]
        [EnableIf("showTicks")]
        public int maxTicks {
            get => _maxTicks;
            set {
                _maxTicks = value;
                UpdateAxes(value, x => x.maxTicks);
            }
        }
        #endregion

        #region public int maxDecimals;
        [SerializeField, HideInInspector]
        private int _maxDecimals = 2;

        [ShowInInspector]
        [PropertyRange(0, 10)]
        [EnableIf("showTicks")]
        public int maxDecimals {
            get => _maxDecimals;
            set {
                _maxDecimals = value;
                UpdateAxes(value, x => x.maxDecimals);
            }
        }
        #endregion

        #region public float offset;
        [SerializeField, HideInInspector]
        private float _offset = 2;

        [ShowInInspector]
        [EnableIf("showTicks")]
        public float offset {
            get => _offset;
            set {
                _offset = value;
                UpdateAxes(value, x => x.tickOffset);
            }
        }
        #endregion

        #region public List<Axis.TickData> manualTicks;
        [SerializeField, HideInInspector]
        private List<Axis.TickData> _manualTicks = new();

        [ShowInInspector]
        [EnableIf("showTicks")]
        public List<Axis.TickData> manualTicks {
            get => _manualTicks;
            set {
                _manualTicks = value;
                UpdateAxes(value, x => x.manualTicks);
            }
        }
        #endregion

        #region public PrefabProvider<AxisTick> ticksPrefab;
        [SerializeField, HideInInspector]
        private PrefabProvider<AxisTick> _ticksPrefab = null;

        [ShowInInspector]
        [RequiredIn(PrefabKind.PrefabAsset)]
        [EnableIf("showTicks")]
        [InlineProperty]
        public PrefabProvider<AxisTick> ticksPrefab {
            get => _ticksPrefab;
            set {
                _ticksPrefab = value;
                UpdateAxes(value, x => x.tickPrefab);
            }
        }
        #endregion


        public void SetAxes(Action onDomainChange, params Axis[] incoming)
        {
            if (!AreAxesValid(incoming) || !IsSomeAxisNew(onDomainChange))
                return;

            scale = axes[0].scale;
            padding = axes[0].padding;
            rodThickness = axes[0].thickness;
            arrowPrefab = axes[0].arrowPrefab;
            arrowPresence = axes[0].arrowPresence;
            showTicks = axes[0].showTicks;
            showZero = axes[0].showZero;
            lockTickOrientation = axes[0].lockTickOrientation;
            ticksPrefab = axes[0].tickPrefab;
            tickSteps = axes[0].step;
            maxTicks = axes[0].maxTicks;
            maxDecimals = axes[0].maxDecimals;
            offset = axes[0].tickOffset;
            manualTicks = axes[0].manualTicks.ToList();

            onDomainChange?.Invoke();
        }

        private bool AreAxesValid(Axis[] incoming)
        {
            if (axes is not null && incoming.SequenceEqual(axes))
                return false;

            axes = incoming.Where(x => x is not null).ToArray();
            return axes.Length != 0;
        }

        private bool IsSomeAxisNew(Action onDomainChange)
        {
            lastOnDomainChange = onDomainChange;

            var hasNewAxis = false;

            for (var i = 0; i < axes.Length; i++) {
                if (axes[i].ListenDomainChange(onDomainChange))
                    hasNewAxis = true;
            }

            return hasNewAxis;
        }


        private void UpdateAxes<TValue>(TValue value, Expression<Func<Axis, TValue>> fieldExpression)
        {
            if (((MemberExpression)fieldExpression.Body).Member is not PropertyInfo prop)
                throw new ArgumentException("The lambda expression should point to a valid Property");

            if (axes is null)
                throw new Exception($"No axis registered in {nameof(MultipleAxesController)}");

            var hasChanges = false;

            for (var i = 0; i < axes.Length; i++) {
                var axis = axes[i];
                var existing = (TValue)prop.GetValue(axis);

                if (ReferenceEquals(value, existing))
                    continue;

                prop.SetValue(axis, value);
                hasChanges = true;
            }

            if (hasChanges)
                lastOnDomainChange();
        }
    }
}
