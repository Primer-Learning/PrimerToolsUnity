using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Axis
{
    [Title("All axes", "Change properties in all axes at once")]
    [HideLabel]
    [InlineProperty]
    [DisableContextMenu]
    [HideReferenceObjectPicker]
    public class MultipleAxesController
    {
        private AxisRenderer[] axes;

        [OnValueChanged(nameof(UpdateRodThickness))]
        public float rodThickness = 1;

        [Required]
        [RequiredIn(PrefabKind.PrefabAsset)]
        [OnValueChanged(nameof(UpdateArrowPrefab))]
        public Transform arrowPrefab;

        [OnValueChanged(nameof(UpdateArrowPresence))]
        public ArrowPresence arrowPresence = ArrowPresence.Both;

        [OnValueChanged(nameof(UpdateShowTicks))]
        public bool showTicks = true;

        [Required]
        [RequiredIn(PrefabKind.PrefabAsset)]
        [EnableIf("showTicks")]
        [OnValueChanged(nameof(UpdateTicksPrefab))]
        public AxisTick ticksPrefab;

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
        [OnValueChanged(nameof(UpdateManualTicks))]
        public List<TicData> manualTicks = new();


        public void SetAxes(Action onDomainChange, params AxisRenderer[] incoming)
        {
            if (AreAxesValid(incoming) || !IsSomeAxisNew(onDomainChange))
                return;

            rodThickness = axes[0].rod.thickness;
            arrowPrefab = axes[0].arrows.prefab;
            arrowPresence = axes[0].arrows.presence;
            showTicks = axes[0].ticks.showTicks;
            ticksPrefab = axes[0].ticks.prefab;
            tickSteps = axes[0].ticks.step;
            maxTicks = axes[0].ticks.maxTicks;
            maxDecimals = axes[0].ticks.maxDecimals;
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
            var hasNewAxis = false;

            for (var i = 0; i < axes.Length; i++) {
                if (axes[i].ListenDomainChange(onDomainChange))
                    hasNewAxis = true;
            }

            return hasNewAxis;
        }


        private void UpdateRodThickness()
            => Update(rodThickness, x => x.rod, x => x.thickness);

        private void UpdateArrowPrefab()
            => Update(arrowPrefab, x => x.arrows, x => x.prefab);

        private void UpdateArrowPresence()
            => Update(arrowPresence, x => x.arrows, x => x.presence);

        private void UpdateShowTicks()
            => Update(showTicks, x => x.ticks, x => x.showTicks);

        private void UpdateTicksPrefab()
            => Update(ticksPrefab, x => x.ticks, x => x.prefab);

        private void UpdateTickSteps()
            => Update(tickSteps, x => x.ticks, x => x.step);

        private void UpdateMaxTicks()
            => Update(maxTicks, x => x.ticks, x => x.maxTicks);

        private void UpdateMaxDecimals()
            => Update(maxDecimals, x => x.ticks, x => x.maxDecimals);

        private void UpdateManualTicks()
            => Update(manualTicks, x => x.ticks, x => x.manualTicks);


        private void Update<TPart, TValue>(
            TValue value,
            Func<AxisRenderer, TPart> partGetter,
            Expression<Func<TPart, TValue>> fieldExpression)
        {
            if (((MemberExpression)fieldExpression.Body).Member is not FieldInfo field)
                throw new ArgumentException("The lambda expression should point to a valid Property");

            for (var i = 0; i < axes.Length; i++) {
                var axis = axes[i];
                var part = partGetter(axis);
                var existing = (TValue)field.GetValue(part);

                if (ReferenceEquals(value, existing))
                    continue;

                field.SetValue(part, value);
                axis.OnValidate();
            }
        }
    }
}
