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

        [Required, ChildGameObjectsOnly]
        public Transform shaft;
        [Required, ChildGameObjectsOnly]
        public Transform head;
        [Required, ChildGameObjectsOnly]
        public Transform tail;

        [Title("Positioning")]
        [DisableIf("@startTracker != null || endTracker != null")]
        [Tooltip("Start and end positions are in global space if true. Start Tracker and End Tracker set this to true.")]
        public bool globalPositioning = false;

        // [HorizontalGroup("range", LabelWidth = 70)]

        [HideLabel, Title("Start", titleAlignment: TitleAlignments.Centered)]
        [DisableIf(nameof(startTracker))]
        [Tooltip("Point where the arrow starts. Start Tracker overrides this value.")]
        public Vector3 start = Vector3.zero;

        [Space]
        [LabelText("Space")]
        public float startSpace = 0;

        [LabelText("Follow")]
        [InlineButton("@startTracker = null", SdfIconType.X, "")]
        [Tooltip("Start of the arrow follow this transform.")]
        public Transform startTracker;

        [LabelText("Pointer")]
        public bool startPointer = false;

        [HideLabel, Title("End", titleAlignment: TitleAlignments.Centered)]
        [DisableIf(nameof(endTracker))]
        [Tooltip("Point where the arrow ends. End Tracker overrides this value.")]
        public Vector3 end = Vector3.one;

        [Space]
        [LabelText("Space")]
        public float endSpace = 0;

        [LabelText("Follow")]
        [InlineButton("@endTracker = null", SdfIconType.X, "")]
        [Tooltip("End of the arrow follow this transform.")]
        public Transform endTracker;

        [LabelText("Pointer")]
        public bool endPointer = true;

        [Space(16)]
        [Title("Fine tuning")]
        public float thickness = 1f;
        public float axisRotation = 0;

        [ShowInInspector]
        [MinValue(0)]
        public float length {
            get => (end - start).magnitude - startSpace - endSpace;
            set => SetLength(value);
        }

        [Title("Constants")]
        [Tooltip("This is the distance for the arrow heads before the shaft starts. " +
            "This only needs to be changed if the arrow mesh changes.")]
        public float arrowLength = 0.18f;


        private float realArrowLength => arrowLength * thickness;


        #region Unity events
        public void OnValidate() => Recalculate();

        public void Update()
        {
            var hasChanges = false;

            if (startTracker != null && startTracker.position != start) {
                start = startTracker.position;
                hasChanges = true;
            }

            if (endTracker != null && endTracker.position != end) {
                end = endTracker.position;
                hasChanges = true;
            }

            if (hasChanges) {
                globalPositioning = true;
                Recalculate();
            }
        }
        #endregion


        public void Follow(GameObject from, GameObject to)
            => Follow(from.transform, to.transform);

        public void Follow(Component from, Component to)
        {
            var fromTransform = from.transform;
            var toTransform = to.transform;

            SetFromTo(fromTransform.position, toTransform.position, true);

            startTracker = fromTransform;
            endTracker = toTransform;
        }

        public void SetFromTo(Vector3 from, Vector3 to, bool global)
        {
            globalPositioning = global;
            SetFromTo(from, to);
        }

        public void SetFromTo(Vector3 from, Vector3 to)
        {
            startTracker = null;
            endTracker = null;
            start = from;
            end = to;
            Recalculate();
        }

        public void SetStartAndEnd(Vector3 start, Vector3? end = null)
        {
            this.start = start;
            this.end = end ?? start;
            Recalculate();
        }

        private void SetLength(float value)
        {
            // If the length is too small, just prevent the change
            if (value < (startPointer ? realArrowLength : 0) + (endPointer ? realArrowLength : 0))
                return;

            var diff = end - start;
            end += (value - diff.magnitude) * Vector3.Normalize(diff);
            Recalculate();
        }


        // ReSharper disable once ParameterHidesMember - the parameter we are hiding is obsolete
        public async UniTask Animate(
            Vector3? from = null,
            Vector3? to = null,
            Tweener animation = null,
            CancellationToken ct = default)
        {
            var tailTo = from ?? start;
            var headTo = to ?? end;

            if (start == tailTo && end == headTo) return;

            if (!Application.isPlaying) {
                start = tailTo;
                end = headTo;
                Recalculate();
                return;
            }

            var tailAnim = (start, end);
            var headAnim = (tailTo, headTo);

            await foreach (var (tailLerped, headLerped) in animation.Tween(tailAnim, headAnim, ct, LerpVector3Pair)) {
                if (ct.IsCancellationRequested) return;
                start = tailLerped;
                end = headLerped;
                Recalculate();
            }

            return;

            static (Vector3, Vector3) LerpVector3Pair((Vector3, Vector3) a, (Vector3, Vector3) b, float t)
            {
                return (
                    Vector3.Lerp(a.Item1, b.Item1, t),
                    Vector3.Lerp(a.Item2, b.Item2, t)
                );
            }
        }


        #region void Recalculate()
        public void Recalculate()
        {
            if (shaft == null || head == null || tail == null || gameObject.IsPreset())
                return;

            startArrowLength = startPointer ? realArrowLength : 0;
            endArrowLength = endPointer ? realArrowLength : 0;
            shaftLength = length - startArrowLength - endArrowLength;

            var scale = this.GetPrimer().ApplyIntrinsicScale(hide: shaftLength <= 0);

            if (scale == Vector3.zero)
                return;

            CalculatePosition();
            CalculateChildrenPosition();
        }

        private void CalculatePosition()
        {
            var arrow = transform;
            var diff = end - start;
            arrow.rotation = Quaternion.FromToRotation(Vector3.right, diff);

            if (globalPositioning)
                arrow.position = (diff / 2) + start;
            else
                arrow.localPosition = (diff / 2) + start;
        }

        private void CalculateChildrenPosition()
        {
            const float EDGE = 0.5f;
            var childRotation = Quaternion.Euler(axisRotation, 0, 0);

            shaft.localPosition = new Vector3((startArrowLength + -endArrowLength) * EDGE, 0, 0);
            shaft.localScale = new Vector3(shaftLength, thickness, thickness);
            shaft.localRotation = childRotation;

            CalculatePointer(childRotation, tail, startPointer, -(length * EDGE) + startArrowLength);
            CalculatePointer(childRotation, head, endPointer, length * EDGE - endArrowLength);
        }

        private void CalculatePointer(Quaternion childRotation, Transform transform, bool show, float x)
        {
            var scale = transform.GetPrimer().ApplyIntrinsicScale(multiplier: thickness, hide: !show);

            if (scale == Vector3.zero)
                return;

            transform.localPosition = new Vector3(x, 0, 0);
            transform.localRotation = childRotation;
        }
        #endregion


        #region Inspector panel
        [OnInspectorGUI] private void Space() => GUILayout.Space(16);

        [ButtonGroup]
        [Button(ButtonSizes.Large, Icon = SdfIconType.Recycle)]
        public void SwapStartEnd()
        {
            (start, end) = (end, start);
            (startTracker, endTracker) = (endTracker, startTracker);
            Recalculate();
        }
        #endregion
    }
}
