using System;
using Sirenix.OdinInspector;
using UnityEngine;

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
                hasDomainChanged = true;
            }
        }
        #endregion

        #region public float scale;
        [SerializeField, HideInInspector]
        private float _scale = 0.2f;

        [ShowInInspector]
        [MinValue(0.1f)]
        public float scale {
            get => _scale;
            set {
                if (_scale == value)
                    return;

                _scale = value;
                hasDomainChanged = true;
            }
        }
        #endregion

        #region public Vector2 padding;
        [SerializeField, HideInInspector]
        private Vector2 _padding = Vector2.zero;

        [ShowInInspector]
        public Vector2 padding {
            get => _padding;
            set {
                _padding = value;
                UpdateChildren();
            }
        }
        #endregion

        internal float start => range.min * scale;
        internal float end => range.max * scale;
        internal float length => end - start;
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
