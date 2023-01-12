using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Axis
{
    [HideLabel]
    [Serializable]
    [InlineProperty]
    [DisableContextMenu]
    [HideReferenceObjectPicker]
    [Title("Domain")]
    internal class AxisDomain
    {
        public Action onChange;

        [SerializeField]
        [OnValueChanged(nameof(Changed))]
        private MinMax range = new() { min = 0, max = 10 };

        [MinValue(0.1f)]
        [OnValueChanged(nameof(Changed))]
        public float length = 1;

        [OnValueChanged(nameof(Changed))]
        public float paddingFraction = 0.05f;

        public float min => range.min;
        public float max => range.max;
        public float positionMultiplier => length * (1 - 2 * paddingFraction) / (range.max - range.min);
        public float offset => -length * paddingFraction + range.min * positionMultiplier;


        protected void Changed() => onChange?.Invoke();


        [Serializable]
        [InlineProperty(LabelWidth = 40)]
        private struct MinMax
        {
            [HorizontalGroup]
            [MaxValue("$max")]
            public float min;

            [HorizontalGroup]
            [MinValue("$min")]
            public float max;
        }
    }
}
