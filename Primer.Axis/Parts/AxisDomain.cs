using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

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

        [FormerlySerializedAs("length")]
        [MinValue(0.1f)]
        [OnValueChanged(nameof(Changed))]
        public float scale = 1;

        [OnValueChanged(nameof(Changed))]
        public Vector2 padding = Vector2.zero;

        public float min => range.min;
        public float max => range.max;
        private float start => range.min * scale;
        private float end => range.max * scale;
        public float length => end - start;
        public float rodStart => start - padding.x;
        public float rodEnd => end + padding.y;


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
