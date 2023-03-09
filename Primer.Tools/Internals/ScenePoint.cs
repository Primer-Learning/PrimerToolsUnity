using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Tools
{
    [Serializable]
    [HideLabel]
    [InlineProperty]
    public struct ScenePoint
    {
        #region bool isWorldPosition;
        [SerializeField, HideInInspector]
        private bool _isWorldPosition;

        [ShowInInspector]
        [DisableIf("follow")]
        public bool isWorldPosition {
            get => isTracking || _isWorldPosition;
            set {
                if (isTracking)
                    follow = null;

                _isWorldPosition = value;
                Changed();
            }
        }
        #endregion

        #region Transform follow;
        [SerializeField, HideInInspector]
        private Transform _follow;

        [ShowInInspector]
        [InlineButton(nameof(StopTracking), SdfIconType.X, "", ShowIf = "_follow")]
        public Transform follow {
            get => _follow;
            set {
                if (value == _follow)
                    return;

                if (value == null)
                    StopTracking();
                else
                    _follow = value;

                Changed();
            }
        }
        #endregion
        
        #region Transform followAdjustmentVector;
        [SerializeField, HideInInspector]
        private Vector3 _followAdjustmentVector;
        [ShowInInspector]
        public Vector3 followAdjustmentVector
        {
            get => _followAdjustmentVector;
            set
            {
                _followAdjustmentVector = value;
                Changed();
            }
        }
        #endregion

        #region Vector3 value;
        [SerializeField, HideInInspector]
        private Vector3 _value;

        [ShowInInspector]
        [DisableIf("follow")]
        public Vector3 value {
            get => isTracking ? follow.position : _value + followAdjustmentVector;
            set {
                StopTracking();

                if (_value == value)
                    return;

                _value = value;
                Changed();
            }
        }
        #endregion

        private Vector3? lastTrackedValue;
        public Action onChange;

        public bool isTracking => _follow != null;


        public bool CheckTrackedObject(bool emitOnChange = true)
        {
            if (!isTracking || lastTrackedValue == follow.position)
                return false;

            lastTrackedValue = follow.position;

            if (emitOnChange)
                Changed();

            return true;
        }

        public void StopTracking()
        {
            if (!isTracking)
                return;

            _value = _follow.position;
            _follow = null;
            lastTrackedValue = null;
        }

        public Vector3 GetLocalPosition(Transform parent)
        {
            return isWorldPosition && parent is not null
                ? parent.InverseTransformPoint(value)
                : value;
        }

        public Vector3 GetWorldPosition(Transform parent)
        {
            return isWorldPosition || parent is null
                ? value
                : parent.TransformPoint(value);
        }

        private void Changed()
        {
            onChange?.Invoke();
        }

        public Func<float, ScenePoint> Tween(Vector3? finalValue, out bool isNoop)
        {
            var start = value;
            var end = finalValue ?? start;
            var isWorld = _isWorldPosition;

            if (start == end || isTracking) {
                var self = this;
                isNoop = true;
                return t => self;
            }

            isNoop = false;
            return t => new ScenePoint {
                _isWorldPosition = isWorld,
                _value = Vector3.Lerp(start, end, t),
            };
        }

        // Statics

        public static bool CheckTrackedObject(params ScenePoint[] points)
        {
            var hasChanges = false;

            for (var i = 0; i < points.Length; i++) {
                if (points[i].CheckTrackedObject(emitOnChange: false))
                    hasChanges = true;
            }

            return hasChanges;
        }

        // Operators

        public static implicit operator Vector3(ScenePoint point)
        {
            return point.value;
        }

        public static implicit operator ScenePoint(Vector3 value)
        {
            return new ScenePoint { value = value };
        }

#if UNITY_EDITOR
        public bool DrawHandle(Transform parent)
        {
            var current = GetWorldPosition(parent);
            var newValue = UnityEditor.Handles.PositionHandle(current, Quaternion.identity);

            if (newValue == current)
                return false;

            StopTracking();
            _isWorldPosition = true;
            _value = newValue;
            return true;
        }
#endif
    }
}
