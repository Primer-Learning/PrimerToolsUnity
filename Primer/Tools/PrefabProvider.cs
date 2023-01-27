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

                stored = value;
                hasChanges = true;

                Nullable<Vector3> a = null;;

                if (value == null) {
                    _position = null;
                    _rotation = null;
                    _scale = null;
                    target = null;
                }
                else {
                    target = value.transform;
                }
            }
        }

        // [HorizontalGroup("details", 70, LabelWidth = 60)]

        // [ShowInInspector]
        // [ShowIf("$stored")]
        // [VerticalGroup("details/left")]
        // [HideLabel]
        // [InlineEditor(
        //     InlineEditorModes.SmallPreview,
        //     Expanded = true,
        //     PreviewHeight = 70,
        //     ObjectFieldMode = InlineEditorObjectFieldModes.Hidden
        // )]
        // private T preview => value;
        #endregion


        #region public Vector3 position = value.localPosition ?? Vector3.zero;
        [SerializeReference]
        private Vector3? _position;

        [ShowInInspector]
        [ShowIf("$value")]
        // [VerticalGroup("details/right")]
        [LabelWidth(60)]
        [InlineButton(nameof(ResetPosition), SdfIconType.Eraser, label: "")]
        public Vector3 position {
            get => _position ?? (target != null ? target.localPosition : Vector3.zero);
            set {
                hasChanges = true;
                _position = value;
            }
        }

        public void ResetPosition() {
            hasChanges = true;
            _position = null;
        }
        #endregion

        #region public Quaternion rotation = value.localRotation ?? Quaternion.identity;
        [SerializeReference]
        private Quaternion? _rotation;

        [ShowInInspector]
        [ShowIf("$value")]
        // [VerticalGroup("details/right")]
        [LabelWidth(60)]
        [InlineButton(nameof(ResetRotation), SdfIconType.Eraser, label: "")]
        public Quaternion rotation {
            get => _rotation ?? (target != null ? target.localRotation : Quaternion.identity);
            set {
                hasChanges = true;
                _rotation = value;
            }
        }

        public void ResetRotation() {
            hasChanges = true;
            _rotation = null;
        }
        #endregion

        #region public Vector3 scale = value.localScale ?? Vector3.one;
        [SerializeReference]
        private Vector3? _scale;

        [ShowInInspector]
        [ShowIf("$value")]
        // [VerticalGroup("details/right")]
        [LabelWidth(60)]
        [InlineButton(nameof(ResetScale), SdfIconType.Eraser, label: "")]
        public Vector3 scale {
            get => _scale ?? (target != null ? target.localScale : Vector3.one);
            set {
                hasChanges = true;
                _scale = value;
            }
        }

        public void ResetScale() {
            hasChanges = true;
            _scale = null;
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

            if (_scale is not null)
                transform.localScale = _scale.Value;

            if (_rotation is not null)
                transform.rotation = _rotation.Value;

            if (_position is not null)
                transform.localPosition = _position.Value;
        }


#if UNITY_EDITOR
        [UsedImplicitly]
        private string type => typeof(T).Name;
#endif
    }
}
