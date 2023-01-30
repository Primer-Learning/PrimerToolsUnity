using System;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Primer
{
    public interface IPrefabProvider<out T> where T : Component
    {
        bool hasChanges { get; set; }
        T value { get; }
    }

    [Serializable]
    [InlineProperty]
    public class PrefabProvider : PrefabProvider<Transform> {}


    [Serializable]
    [InlineProperty]
    public class PrefabProvider<T> : IPrefabProvider<T> where T : Component
    {
        private Transform target;
        public bool hasChanges { get; set; }
        public Func<T> getter = null;

        public bool isEmpty => value == null;


        #region public T value => getter() ?? lastSetValue;
        [SerializeField]
        [HideInInspector]
        private T stored;

        [ShowInInspector]
        // [RequiredIf()]
        [HideLabel]
        public T value {
            get => getter is null ? stored : value = getter();

            set {
                if (Equals(stored, value))
                    return;

                if (value == null) {
                    ResetPosition();
                    ResetRotation();
                    ResetScale();
                    target = null;
                }
                else {
                    target = value.transform;
                }

                stored = value;
                hasChanges = true;
            }
        }

        #endregion


        #region public Vector3 position = value.localPosition ?? Vector3.zero;
        [SerializeReference]
        [HideInInspector]
        private Vector3 _position;
        [HideInInspector]
        public bool hasPosition = false;

        [ShowInInspector]
        [ShowIf("$value")]
        [LabelWidth(60)]
        [InlineButton(nameof(ResetPosition), SdfIconType.Eraser, label: "")]
        public Vector3 position {
            get => hasPosition || target == null ? _position : target.localPosition;
            set {
                hasChanges = true;
                hasPosition = true;
                _position = value;
            }
        }

        public void ResetPosition() {
            hasChanges = true;
            hasPosition = false;
            _position = default(Vector3);
        }
        #endregion

        #region public Quaternion rotation = value.localRotation ?? Quaternion.identity;
        [HideInInspector]
        [SerializeReference]
        private Quaternion _rotation;
        [HideInInspector]
        public bool hasRotation = false;

        [ShowInInspector]
        [ShowIf("$value")]
        [LabelWidth(60)]
        [InlineButton(nameof(ResetRotation), SdfIconType.Eraser, label: "")]
        public Quaternion rotation {
            get => hasRotation || target == null ? _rotation : target.localRotation;
            set {
                hasChanges = true;
                hasRotation = true;
                _rotation = value;
            }
        }

        public void ResetRotation() {
            hasChanges = true;
            hasRotation = false;
            _rotation = default(Quaternion);
        }
        #endregion

        #region public Vector3 scale = value.localScale ?? Vector3.one;
        [SerializeReference]
        [HideInInspector]
        private Vector3 _scale = Vector3.one;
        [HideInInspector]
        public bool hasScale = false;

        [ShowInInspector]
        [ShowIf("$value")]
        [LabelWidth(60)]
        [InlineButton(nameof(ResetScale), SdfIconType.Eraser, label: "")]
        public Vector3 scale {
            get => hasScale || target == null ? _scale : target.localScale;
            set {
                hasChanges = true;
                hasScale = true;
                _scale = value;
            }
        }

        public void ResetScale() {
            hasChanges = true;
            hasScale = false;
            _scale = Vector3.one;
        }
        #endregion


        public T Instantiate(Transform parent)
        {
            var child = UnityEngine.Object.Instantiate(value, parent);
            Initialize(child);
            return child;
        }

        public void Initialize(T child)
        {
            var transform = child.transform;

            if (hasScale)
                transform.localScale = _scale;

            if (hasRotation)
                transform.rotation = _rotation;

            if (hasPosition)
                transform.localPosition = _position;
        }


#if UNITY_EDITOR
        [UsedImplicitly]
        private string type => typeof(T).Name;
#endif
    }
}
