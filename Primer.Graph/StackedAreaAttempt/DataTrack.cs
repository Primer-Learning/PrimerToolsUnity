using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Primer.Graph
{
    [Serializable]
    public class DataTrack
    {
        [SerializeField, HideInInspector]
        private string _name;
        internal Action<string> onNameChange;

        [ShowInInspector]
        public string name {
            get => _name;
            set => Meta.ReactiveProp(ref _name, value, onNameChange);
        }

        [SerializeField, HideInInspector]
        private Color _color;
        internal Action<Color> onColorChange;

        [ShowInInspector]
        public Color color {
            get => _color;
            set => Meta.ReactiveProp(ref _color, value, onColorChange);
        }

        [FormerlySerializedAs("_value")] [SerializeField, HideInInspector]
        private List<float> _values;
        internal Action<IReadOnlyList<float>> onValuesChange;

        [ShowInInspector]
        public IReadOnlyList<float> values {
            get => _values;
            set => Meta.ReactiveProp(ref _values, value.ToList(), onValuesChange);
        }

        public void AddValue(float value)
        {
            _values.Add(value);
            onValuesChange?.Invoke(_values);
        }
    }
}
