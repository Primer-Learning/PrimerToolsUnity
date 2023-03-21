using Primer.Animation;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Component = UnityEngine.Component;

namespace Primer.Tools
{
    [ExecuteAlways]
    public class PrimerArrow2 : MonoBehaviour
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
        [Title("Start")]
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
        [Title("End")]
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
        public float length {
            get => (head  - tail).magnitude - tailSpace - headSpace;
            set => SetLength(value);
        }

        [Title("Constants")]
        [Tooltip("This is the distance for the arrow heads before the shaft starts. " +
            "This only needs to be changed if the arrow mesh changes.")]
        public float arrowLength = 0.18f;

        public bool globalPositioning {
            get => tailPoint.isWorldPosition || headPoint.isWorldPosition;
            set {
                tailPoint.isWorldPosition = value;
                headPoint.isWorldPosition = value;
            }
        }


        public Vector3 tail => tailPoint.GetWorldPosition(transform.parent);
        public Vector3 head => headPoint.GetWorldPosition(transform.parent);
        private float realArrowLength => arrowLength * thickness;


        #region Unity events
        public void OnEnable()
        {
            // TODO: Scheduler doesn't work
            // var scheduler = new Scheduler(Recalculate);
            // startPoint.onChange = scheduler.Schedule;
            // endPoint.onChange = scheduler.Schedule;
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
            if (ScenePoint.CheckTrackedObject(tailPoint, headPoint)) {
                Recalculate();
            }
        }
        #endregion


        #region Setters
        public PrimerArrow2 Follow(GameObject from, GameObject to) => Follow(from.transform, to.transform);
        public PrimerArrow2 Follow(Component from, Component to)
        {
            tailPoint.follow = from.transform;
            headPoint.follow = to.transform;
            return this;
        }

        public void SetFromTo(Vector3 from, Vector3 to, bool global)
        {
            globalPositioning = global;
            SetFromTo(from, to);
        }

        public void SetFromTo(Vector3 from, Vector3 to)
        {
            tailPoint.vector = from;
            headPoint.vector = to;
        }

        private void SetLength(float value)
        {
            // If the length is too small, just prevent the change
            if (value < (tailPointer ? realArrowLength : 0) + (headPointer ? realArrowLength : 0))
                return;

            var diff = head - tail;
            headPoint.vector += (value - diff.magnitude) * Vector3.Normalize(diff);
        }
        #endregion


        #region Animations
        public Tween GrowFromStart(Vector3Provider headPos, Vector3Provider tailPos)
        {
            return Animate(
                tailStart: tailPos,
                headStart: tailPos,
                tailEnd:tailPos,
                headEnd: headPos
            );
        }

        public Tween ShrinkToEnd()
        {
            tailPoint.StopTracking();
            headPoint.StopTracking();
            return Animate(tailEnd: head);
        }

        public Tween Animate(
            Vector3Provider headEnd = null,
            Vector3Provider tailEnd = null,
            Vector3Provider headStart = null,
            Vector3Provider tailStart = null
        ) {
            var tailTween = tailPoint.Tween(tailEnd, tailStart);
            var headTween = headPoint.Tween(headEnd, headStart);

            return new Tween(t => {
                if (tailTween is not null)
                    tailPoint.vector = tailTween(t);

                if (headTween is not null)
                    headPoint.vector = headTween(t);

                Recalculate();
            });
        }
        #endregion


        #region void Recalculate()
        public void Recalculate()
        {
            if (shaftObject == null || headObject == null || tailObject == null || gameObject.IsPreset())
                return;

            startArrowLength = tailPointer ? realArrowLength : 0;
            endArrowLength = headPointer ? realArrowLength : 0;
            shaftLength = length - startArrowLength - endArrowLength;

            var scale = this.GetPrimer().FindIntrinsicScale();

            if (shaftLength <= 0) {
                transform.localScale = Vector3.zero;
                return;
            }

            transform.localScale = scale;
            CalculatePosition();
            CalculateChildrenPosition();
        }

        private void CalculatePosition()
        {
            var arrow = transform;
            var diff = head - tail;

            arrow.SetGlobalScale(Vector3.one);
            arrow.rotation = Quaternion.FromToRotation(Vector3.right, diff);
            arrow.position = diff / 2 + tail;
        }

        private void CalculateChildrenPosition()
        {
            var childRotation = Quaternion.Euler(axisRotation, 0, 0);
            var shaftMiddle = (tailSpace + startArrowLength) / 2 - (headSpace + endArrowLength) / 2;

            shaftObject.localPosition = new Vector3(shaftMiddle, 0, 0);
            shaftObject.localScale = new Vector3(shaftLength, thickness, thickness);
            shaftObject.localRotation = childRotation;

            var edge = (head - tail).magnitude / 2;

            tailObject.gameObject.SetActive(tailPointer);
            headObject.gameObject.SetActive(headPointer);

            if (tailPointer)
                CalculatePointer(tailObject, childRotation, -(edge - tailSpace - startArrowLength));

            if (headPointer)
                CalculatePointer(headObject, childRotation, edge - headSpace - endArrowLength);
        }

        private void CalculatePointer(Transform pointer, Quaternion childRotation, float x)
        {
            pointer.localScale = pointer.GetPrimer().FindIntrinsicScale() * thickness;
            pointer.localPosition = new Vector3(x, 0, 0);
            pointer.localRotation = childRotation;
        }
        #endregion


        #region Inspector panel
        [OnInspectorGUI] private void Space() => GUILayout.Space(16);

        [ButtonGroup]
        [Button(ButtonSizes.Large, Icon = SdfIconType.Recycle)]
        public void SwapStartEnd()
        {
            (tailPoint, headPoint) = (headPoint, tailPoint);
            Recalculate();
        }
        #endregion
    }
}
