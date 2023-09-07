using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Primer.Graph
{
    public partial class Axis
    {
        #region public MinMax range;
        [SerializeField, HideInInspector]
        private MinMax _range = new() { min = 0, max = 10 };

        [Title("Domain")]
        [ShowInInspector]
        private MinMax range {
            get => _range;
            set {
                if (range.min == value.min && range.max == value.max)
                    return;

                _range = value;
                _scale = _length / (range.max - range.min);
                hasDomainChanged = true;
            }
        }
        #endregion

        #region public float scale;
        // Scale has been made internal. It's not a useful quantity to think about when making decisions about
        // how a graph appears in a scene. Range and length are more useful.
        // TODO: Eliminate scale as a field and make it always be calculated from range and length.
        private float _scale = 0.2f;
        internal float scale {
            get => _scale;
            set => length = value * (range.max - range.min);
        }
        #endregion

        [FormerlySerializedAs("_padding")]
        public Vector2 padding = Vector2.zero;

        internal float start => range.min * scale;
        internal float end => range.max * scale;

        [SerializeField, HideInInspector]
        private float _length = 1;
        [ShowInInspector]
        [MinValue(0.1f)]
        public float length
        {
            get => _length;
            set
            {
                if (_length == value)
                    return;
                
                _length = value;
                _scale = value / (range.max - range.min);
                hasDomainChanged = true;
            }
        }

        internal float rodStart => start - padding.x;
        internal float rodEnd => end + padding.y;

        public float min {
            get => _range.min;
            set => _range.min = value;
        }

        public float max {
            get => _range.max;
            set => _range.max = value;
        }


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

            public MinMax(float min, float max)
            {
                this.min = min;
                this.max = max;
            }
        }
    }
}
