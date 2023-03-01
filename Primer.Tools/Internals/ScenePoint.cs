using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Tools
{
    [Serializable]
    [InlineProperty]
    public class ScenePoint
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

        #region Tranform follow;
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

        #region Vector3 value;
        [SerializeField, HideInInspector]
        private Vector3 _value;

        [ShowInInspector]
        [DisableIf("follow")]
        public Vector3 value {
            get => isTracking ? follow.position : _value;
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

        public bool isLocalPosition {
            get => !isWorldPosition;
            set => isWorldPosition = !value;
        }


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

        // Statics

        public static void Listen(Action listener, params ScenePoint[] points)
        {
            for (var i = 0; i < points.Length; i++) {
                points[i].onChange = listener;
            }
        }

        public static void StopListening(params ScenePoint[] points)
        {
            for (var i = 0; i < points.Length; i++) {
                points[i].onChange = null;
            }
        }

        public static bool CheckTrackedObject(params ScenePoint[] points)
        {
            var hasChanges = false;

            for (var i = 0; i < points.Length; i++) {
                if (points[i].CheckTrackedObject(emitOnChange: false))
                    hasChanges = true;
            }

            return hasChanges;
        }

        public static Vector3[] GetRelativeValues(Transform parent, params ScenePoint[] points)
        {
            var worldPositions = 0;

            for (var i = 0; i < points.Length; i++) {
                if (points[i].isWorldPosition)
                    worldPositions++;
                else
                    worldPositions--;
            }

            var useWorldPosition = worldPositions > 0;
            var values = new Vector3[points.Length];

            for (var i = 0; i < points.Length; i++) {
                values[i] = useWorldPosition
                    ? points[i].GetWorldPosition(parent)
                    : points[i].GetLocalPosition(parent);
            }

            return values;
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

        public static Vector3 operator -(ScenePoint a, ScenePoint b)
        {
            return a.value - b.value;
        }
    }
}
