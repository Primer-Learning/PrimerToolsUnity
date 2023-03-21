using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

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
        public bool isWorldPosition {
            get => _isWorldPosition;
            set {
                _isWorldPosition = value;
                CheckTrackedObject(emitOnChange: false);
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

        public Vector3 followedValue => isWorldPosition ? follow.position : follow.localPosition;
        #endregion

        #region Vector3 value;
        [FormerlySerializedAs("_value")]
        [SerializeField, HideInInspector]
        private Vector3 _vector;

        [ShowInInspector]
        [DisableIf("follow")]
        public Vector3 vector {
            get => _vector;
            set {
                StopTracking();

                if (_vector == value)
                    return;

                _vector = value;
                Changed();
            }
        }
        #endregion

        #region Vector3 followAdjustmentVector;
        [FormerlySerializedAs("_followAdjustmentVector")]
        [SerializeField, HideInInspector]
        private Vector3 _adjustment;

        [ShowInInspector]
        [InlineButton(
            nameof(ResetAdjustment),
            SdfIconType.X,
            "",
            ShowIf = nameof(hasAdjustment)
        )]
        public Vector3 adjustment {
            get => _adjustment;
            set {
                if (_adjustment == value)
                    return;

                _adjustment = value;
                Changed();
            }
        }

        private bool hasAdjustment => adjustment.x != 0 || adjustment.y != 0 || adjustment.z != 0;

        private void ResetAdjustment()
        {
            adjustment = Vector3.zero;
            Changed();
        }
        #endregion

        public Action onChange;

        public bool isTracking => _follow != null;

        /// <summary>
        /// This may return the local or world position coordinates.
        /// You probably want to use GetLocalPosition or GetWorldPosition instead.
        /// </summary>
        [ShowInInspector]
        private Vector3 value => (isTracking ? followedValue : _vector) + adjustment;


        public bool CheckTrackedObject(bool emitOnChange = true)
        {
            if (!isTracking || _vector == followedValue)
                return false;

            _vector = followedValue;

            if (emitOnChange)
                Changed();

            return true;
        }

        public void StopTracking()
        {
            if (!isTracking)
                return;

            _vector = followedValue;
            _follow = null;
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

        public Func<float, Vector3> Tween(Vector3Provider to = null, Vector3Provider from = null)
        {
            var start = from ?? value;
            var end = to ?? value;
            return t => Vector3.Lerp(start, end, t);
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
            return new ScenePoint { vector = value };
        }

#if UNITY_EDITOR
        public bool DrawHandle(Transform parent)
        {
            var current = GetWorldPosition(parent);
            var newValue = UnityEditor.Handles.PositionHandle(current, Quaternion.identity);

            if (newValue == current)
                return false;

            StopTracking();
            _vector = isWorldPosition ? newValue : parent.InverseTransformPoint(newValue);

            return true;
        }
#endif
    }
}
