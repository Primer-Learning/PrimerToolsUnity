using System;
using UnityEngine;
using UnityEngine.Serialization;

// From
// https://www.youtube.com/watch?v=uZmWgQ7cLNI
// https://gist.github.com/aarthificial/f2dbb58e4dbafd0a93713a380b9612af

namespace Primer
{
    [Serializable]
    public struct Optional<T>
    {
        [SerializeField] private bool _enabled;
        [SerializeField] private T _value;

        public bool enabled => _enabled;
        public T value => _value;

        public Optional(T initialValue)
        {
            _enabled = true;
            _value = initialValue;
        }

        public static implicit operator Optional<T>(T value) => new(value);
    }
}
