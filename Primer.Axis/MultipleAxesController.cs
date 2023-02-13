using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Axis
{
    [ExecuteAlways]
    public class MultipleAxesController : MonoBehaviour
    {
        [NonSerialized]
        private AxisRenderer[] axes;

        [NonSerialized]
        private Action lastOnDomainChange;

        [Title("All axes", "Change properties in all axes at once")]
        [MinValue(0.1f)]
        [OnValueChanged(nameof(UpdateScale))]
        public float scale = 1;

        [OnValueChanged(nameof(UpdatePadding))]
        private Vector2 padding = Vector2.zero;

        [OnValueChanged(nameof(UpdateRodThickness))]
        public float rodThickness = 1;

        [Title("Arrows")]
        [OnValueChanged(nameof(UpdateArrowPresence))]
        public ArrowPresence arrowPresence = ArrowPresence.Both;

        [InlineProperty]
        [RequiredIn(PrefabKind.PrefabAsset)]
        [OnValueChanged(nameof(UpdateArrowPrefab))]
        public PrefabProvider arrowPrefab;

        [Title("Ticks")]
        [OnValueChanged(nameof(UpdateShowTicks))]
        public bool showTicks = true;

        [OnValueChanged(nameof(UpdateShowZero))]
        public bool showZero = true;

        [MinValue(0.1f)]
        [EnableIf("showTicks")]
        [DisableIf("@manualTicks.Count != 0")]
        [OnValueChanged(nameof(UpdateTickSteps))]
        public float tickSteps = 2;

        [Range(1, 100)]
        [EnableIf("showTicks")]
        [OnValueChanged(nameof(UpdateMaxTicks))]
        public int maxTicks = 50;

        [Range(0, 10)]
        [EnableIf("showTicks")]
        [OnValueChanged(nameof(UpdateMaxDecimals))]
        public int maxDecimals = 2;

        [EnableIf("showTicks")]
        [OnValueChanged(nameof(UpdateTickOffset))]
        public float offset = 2;

        [EnableIf("showTicks")]
        [OnValueChanged(nameof(UpdateManualTicks))]
        public List<TicData> manualTicks = new();

        [RequiredIn(PrefabKind.PrefabAsset)]
        [EnableIf("showTicks")]
        [OnValueChanged(nameof(UpdateTicksPrefab))]
        [InlineProperty]
        public PrefabProvider<AxisTick> ticksPrefab;


        public void SetAxes(Action onDomainChange, params AxisRenderer[] incoming)
        {
            if (!AreAxesValid(incoming) || !IsSomeAxisNew(onDomainChange))
                return;

            scale = axes[0].domain.scale;
            padding = axes[0].domain.padding;
            rodThickness = axes[0].rod.thickness;
            arrowPrefab = axes[0].arrows.prefab;
            arrowPresence = axes[0].arrows.presence;
            showTicks = axes[0].ticks.showTicks;
            showZero = axes[0].ticks.showZero;
            ticksPrefab = axes[0].ticks.prefab;
            tickSteps = axes[0].ticks.step;
            maxTicks = axes[0].ticks.maxTicks;
            maxDecimals = axes[0].ticks.maxDecimals;
            offset = axes[0].ticks.offset;
            manualTicks = axes[0].ticks.manualTicks.ToArray().ToList();

            onDomainChange?.Invoke();
        }

        private bool AreAxesValid(AxisRenderer[] incoming)
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


        private void UpdateScale() => UpdateAxes(scale, x => x.domain, x => x.scale);
        private void UpdatePadding() => UpdateAxes(padding, x => x.domain, x => x.padding);
        private void UpdateRodThickness() => UpdateAxes(rodThickness, x => x.rod, x => x.thickness);
        private void UpdateArrowPrefab() => UpdateAxes(arrowPrefab, x => x.arrows, x => x.prefab);
        private void UpdateArrowPresence() => UpdateAxes(arrowPresence, x => x.arrows, x => x.presence);
        private void UpdateShowTicks() => UpdateAxes(showTicks, x => x.ticks, x => x.showTicks);
        private void UpdateShowZero() => UpdateAxes(showZero, x => x.ticks, x => x.showZero);
        private void UpdateTicksPrefab() => UpdateAxes(ticksPrefab, x => x.ticks, x => x.prefab);
        private void UpdateTickSteps() => UpdateAxes(tickSteps, x => x.ticks, x => x.step);
        private void UpdateMaxTicks() => UpdateAxes(maxTicks, x => x.ticks, x => x.maxTicks);
        private void UpdateMaxDecimals() => UpdateAxes(maxDecimals, x => x.ticks, x => x.maxDecimals);
        private void UpdateTickOffset() => UpdateAxes(offset, x => x.ticks, x => x.offset);
        private void UpdateManualTicks() => UpdateAxes(manualTicks, x => x.ticks, x => x.manualTicks);


        private void UpdateAxes<TPart, TValue>(
            TValue value,
            Func<AxisRenderer, TPart> partGetter,
            Expression<Func<TPart, TValue>> fieldExpression)
        {
            if (((MemberExpression)fieldExpression.Body).Member is not FieldInfo field)
                throw new ArgumentException("The lambda expression should point to a valid Property");

            if (axes is null)
                throw new Exception($"No axis registered in {nameof(MultipleAxesController)}");

            var hasChanges = false;

            for (var i = 0; i < axes.Length; i++) {
                var axis = axes[i];
                var part = partGetter(axis);
                var existing = (TValue)field.GetValue(part);

                if (ReferenceEquals(value, existing))
                    continue;

                field.SetValue(part, value);
                axis.OnValidate();
                hasChanges = true;
            }

            if (hasChanges)
                lastOnDomainChange();
        }
    }
}
