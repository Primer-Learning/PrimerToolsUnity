using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Primer.Shapes
{
    [ExecuteAlways]
    public partial class PrimerArrow2 : MonoBehaviour
    {
        private float shaftLength;
        private float startArrowLength;
        private float endArrowLength;

        [FormerlySerializedAs("shaft")]
        [SerializeField, PrefabChild]
        private Transform shaftObject;
        [FormerlySerializedAs("head")]
        [SerializeField, PrefabChild]
        private Transform headObject;
        [FormerlySerializedAs("tail")]
        [SerializeField, PrefabChild]
        private Transform tailObject;

        [FormerlySerializedAs("startPoint")]
        [Title("Tail")]
        public ScenePoint tailPoint = Vector3.zero;
        [FormerlySerializedAs("startSpace")]
        [LabelText("Space")]
        [OnValueChanged(nameof(Recalculate))]
        public float tailSpace = 0;
        [FormerlySerializedAs("startPointer")]
        [LabelText("Pointer")]
        [OnValueChanged(nameof(Recalculate))]
        public bool tailPointer = false;

        [FormerlySerializedAs("endPoint")]
        [Title("Head")]
        public ScenePoint headPoint = Vector3.one;
        [FormerlySerializedAs("endSpace")]
        [LabelText("Space")]
        [OnValueChanged(nameof(Recalculate))]
        public float headSpace = 0;
        [FormerlySerializedAs("endPointer")]
        [LabelText("Pointer")]
        [OnValueChanged(nameof(Recalculate))]
        public bool headPointer = true;

        [Space(16)]
        [Title("Fine tuning")]
        [OnValueChanged(nameof(Recalculate))]
        public float thickness = 1f;
        [OnValueChanged(nameof(Recalculate))]
        public float axisRotation = 0;

        [ShowInInspector]
        [MinValue(0)]
        public float length
        {
            get => (head - tail).magnitude - tailSpace - headSpace;
            set => SetLength(value);
        }

        [Title("Constants")]
        [Tooltip("This is the distance for the arrow heads before the shaft starts. " +
            "This only needs to be changed if the arrow mesh changes.")]
        public float arrowLength = 0.05f;

        public bool globalPositioning
        {
            get => tailPoint.isWorldPosition || headPoint.isWorldPosition;
            set
            {
                tailPoint.isWorldPosition = value;
                headPoint.isWorldPosition = value;
            }
        }


        public Vector3 tail {
            get => tailPoint.GetWorldPosition(transform.parent);
            set => tailPoint.SetWorldPosition(transform.parent, value);
        }

        public Vector3 head {
            get => headPoint.GetWorldPosition(transform.parent);
            set => headPoint.SetWorldPosition(transform.parent, value);
        }

        private float realArrowLength => arrowLength * thickness;


        private void SetDefaults()
        {
            tailPoint = Vector3.zero;
            headPoint = Vector3.one;
            tailSpace = 0;
            headSpace = 0;
            tailPointer = false;
            headPointer = true;
            thickness = 1f;
            axisRotation = 0;
            arrowLength = 0.05f;
        }


        #region Unity events
        public void OnEnable()
        {
            tailPoint.onChange = Recalculate;
            headPoint.onChange = Recalculate;
        }

        public void OnDisable()
        {
            tailPoint.onChange = null;
            headPoint.onChange = null;
        }

        public void Update()
        {
            if (ScenePoint.CheckTrackedObject(tailPoint, headPoint))
            {
                Recalculate();
            }
        }
        #endregion


        #region Inspector panel
        // [OnInspectorGUI] private void Space() => GUILayout.Space(16);

        [PropertySpace]
        [Button(ButtonSizes.Medium, Icon = SdfIconType.ArrowRepeat)]
        private void Refresh()
        {
            Recalculate();
        }

        [PropertySpace]
        [Button(ButtonSizes.Large, Icon = SdfIconType.Recycle)]
        public void SwapStartEnd()
        {
            (tailPoint, headPoint) = (headPoint, tailPoint);
            Recalculate();
        }
        #endregion
    }
}
