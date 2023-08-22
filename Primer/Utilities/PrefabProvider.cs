using System;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Primer
{
    [Serializable]
    [InlineProperty]
    public class PrefabProvider : PrefabProvider<Transform> {}


    /// <summary>
    ///     This class is used to get a prefab with a modified position, rotation and scale.
    ///     Instead of adding `Transform` fields to your component, you can use PrefabProvider&lt;Transform&gt;.
    ///     This will add fields in the inspector so the user can specify a rotation or a scale for
    ///         this usage of the prefab only.
    /// </summary>
    /// <remarks>
    ///     To get the prefab instance use the `Instantiate(parent)` method.
    ///     If you instantiated the prefab with your own method, you can use the `Initialize(child)` method to apply the
    ///         position, rotation and scale to the child.
    /// </remarks>
    /// <typeparam name="T">
    ///     A component the prefab must contain.
    ///     Use Transform if you don't care.
    /// </typeparam>
    [Serializable]
    [InlineProperty]
    [HideReferenceObjectPicker]
    public class PrefabProvider<T> where T : Component
    {
        private Transform target;
        private Func<T> getter = null;
        public bool hasChanges { get; set; }
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

        /// <summary>Creates a new instance of the prefab in the given parent.</summary>
        public T Instantiate(Transform parent)
        {
            var child = UnityEngine.Object.Instantiate(value, parent);
            Initialize(child);
            return child;
        }

        /// <summary>
        ///     Applies the position, rotation and scale to the child. This is used internally by `Instantiate(parent)`
        /// </summary>
        public void Initialize(T child)
        {
            var transform = child.transform;

            if (hasScale)
                transform.localScale = _scale;

            if (hasRotation)
                transform.localRotation = _rotation;

            if (hasPosition)
                transform.localPosition = _position;
        }


#if UNITY_EDITOR
        [UsedImplicitly]
        private string type => typeof(T).Name;
#endif
    }
}
