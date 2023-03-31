using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    /// <summary>
    ///     This class contains the data for the a bar in the graph
    ///     And will notify the parent BarPlot when the data changes
    /// </summary>
    [Serializable]
    public class BarData
    {
        [SerializeField, HideInInspector]
        private string _name;
        internal Action<string> onNameChange;

        [ShowInInspector]
        public string name {
            get => _name;
            set {
                if (value == _name) return;
                _name = value;
                onNameChange?.Invoke(value);
            }
        }


        [SerializeField, HideInInspector]
        private float _value;
        internal Action<float> onValueChange;

        [ShowInInspector]
        public float value {
            get => _value;
            set {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (value == _value) return;
                _value = value;
                onValueChange?.Invoke(value);
            }
        }


        [SerializeField, HideInInspector]
        private Color _color;
        internal Action<Color> onColorChange;

        [ShowInInspector]
        public Color color {
            get => _color;
            set {
                if (value == _color) return;
                _color = value;
                onColorChange?.Invoke(value);
            }
        }
    }
}
