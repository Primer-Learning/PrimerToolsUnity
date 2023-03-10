using System.Threading;
using Cysharp.Threading.Tasks;
using Primer.Animation;
using Sirenix.OdinInspector;
using UnityEngine;
using Component = UnityEngine.Component;

namespace Primer.Tools
{
    [ExecuteAlways]
    public class PrimerArrow2 : MonoBehaviour
    {
        private float shaftLength;
        private float startArrowLength;
        private float endArrowLength;

        [SerializeField, PrefabChild]
        private Transform shaft;
        [SerializeField, PrefabChild]
        private Transform head;
        [SerializeField, PrefabChild]
        private Transform tail;

        [Title("Start")]
        public ScenePoint startPoint = Vector3.zero;
        [LabelText("Space")]
        public float startSpace = 0;
        [LabelText("Pointer")]
        public bool startPointer = false;

        [Title("End")]
        public ScenePoint endPoint = Vector3.one;
        [LabelText("Space")]
        public float endSpace = 0;
        [LabelText("Pointer")]
        public bool endPointer = true;

        [Space(16)]
        [Title("Fine tuning")]
        public float thickness = 1f;
        public float axisRotation = 0;

        [ShowInInspector]
        [MinValue(0)]
        public float length {
            get => (end  - start).magnitude - startSpace - endSpace;
            set => SetLength(value);
        }

        [Title("Constants")]
        [Tooltip("This is the distance for the arrow heads before the shaft starts. " +
            "This only needs to be changed if the arrow mesh changes.")]
        public float arrowLength = 0.18f;

        public bool globalPositioning {
            get => startPoint.isWorldPosition || endPoint.isWorldPosition;
            set {
                startPoint.isWorldPosition = value;
                endPoint.isWorldPosition = value;
            }
        }


        public Vector3 start => startPoint.GetWorldPosition(transform.parent);
        public Vector3 end => endPoint.GetWorldPosition(transform.parent);
        private float realArrowLength => arrowLength * thickness;


        #region Unity events
        public void OnEnable()
        {
            // TODO: Scheduler doesn't work
            // var scheduler = new Scheduler(Recalculate);
            // startPoint.onChange = scheduler.Schedule;
            // endPoint.onChange = scheduler.Schedule;
            startPoint.onChange = Recalculate;
            endPoint.onChange = Recalculate;
        }

        public void OnDisable()
        {
            startPoint.onChange = null;
            endPoint.onChange = null;
        }

        public void Update()
        {
            if (ScenePoint.CheckTrackedObject(startPoint, endPoint)) {
                Recalculate();
            }
        }
        #endregion


        #region Setters
        public void Follow(GameObject from, GameObject to) => Follow(from.transform, to.transform);
        public void Follow(Component from, Component to)
        {
            startPoint.follow = from.transform;
            endPoint.follow = to.transform;
        }

        public void SetFromTo(Vector3 from, Vector3 to, bool global)
        {
            globalPositioning = global;
            SetFromTo(from, to);
        }

        public void SetFromTo(Vector3 from, Vector3 to)
        {
            startPoint.value = from;
            endPoint.value = to;
        }

        private void SetLength(float value)
        {
            // If the length is too small, just prevent the change
            if (value < (startPointer ? realArrowLength : 0) + (endPointer ? realArrowLength : 0))
                return;

            var diff = end - start;
            endPoint.value += (value - diff.magnitude) * Vector3.Normalize(diff);
        }
        #endregion


        #region Animations
        public Tween GrowFromStart(Vector3 from, Vector3 to)
        {
            SetFromTo(from, from);
            return Animate(from, to);
        }

        public Tween ShrinkToEnd()
        {
            startPoint.StopTracking();
            endPoint.StopTracking();
            return Animate(end, end);
        }

        // ReSharper disable once ParameterHidesMember - the parameter we are hiding is obsolete
        public Tween Animate(Vector3? from = null, Vector3? to = null)
        {
            var startTween = startPoint.Tween(from, out var isStartNoop);
            var endTween = startPoint.Tween(from, out var isEndNoop);

            return isStartNoop || isEndNoop
                ? Tween.noop
                : new Tween(t => {
                    startPoint = startTween(t);
                    endPoint = endTween(t);
                    Recalculate();
                });
        }
        #endregion


        #region void Recalculate()
        public void Recalculate()
        {
            if (shaft == null || head == null || tail == null || gameObject.IsPreset())
                return;

            startArrowLength = startPointer ? realArrowLength : 0;
            endArrowLength = endPointer ? realArrowLength : 0;
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
            var diff = end - start;

            arrow.SetGlobalScale(Vector3.one);
            arrow.rotation = Quaternion.FromToRotation(Vector3.right, diff);
            arrow.position = diff / 2 + start;
        }

        private void CalculateChildrenPosition()
        {
            var childRotation = Quaternion.Euler(axisRotation, 0, 0);
            var shaftMiddle = (startSpace + startArrowLength) / 2 - (endSpace + endArrowLength) / 2;

            shaft.localPosition = new Vector3(shaftMiddle, 0, 0);
            shaft.localScale = new Vector3(shaftLength, thickness, thickness);
            shaft.localRotation = childRotation;

            var edge = (end - start).magnitude / 2;

            tail.gameObject.SetActive(startPointer);
            head.gameObject.SetActive(endPointer);

            if (startPointer)
                CalculatePointer(tail, childRotation, -(edge - startSpace - startArrowLength));

            if (endPointer)
                CalculatePointer(head, childRotation, edge - endSpace - endArrowLength);
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
            (startPoint.value, endPoint.value) = (endPoint.value, startPoint.value);
            (startPoint.follow, endPoint.follow) = (endPoint.follow, startPoint.follow);
        }
        #endregion
    }
}
