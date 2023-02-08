using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Tools
{
    [ExecuteAlways]
    public class PrimerArrow2 : MonoBehaviour
    {
        private float shaftLength;

        [Required]
        [ChildGameObjectsOnly]
        public Transform shaft;

        [Required]
        [ChildGameObjectsOnly]
        public Transform head;

        [Required]
        [ChildGameObjectsOnly]
        public Transform tail;

        [Title("Positioning")]
        [InlineButton(nameof(SwapStartEnd))]
        public bool globalPositioning = false;
        public Vector3 start = Vector3.zero;
        public Vector3 end = Vector3.one;
        public Vector2 buffer = Vector2.zero;
        public Transform startTracker;
        public Transform endTracker;

        [Title("Fine tuning")]
        public float thickness = 1f;
        public float arrowLength = 0.18f;

        [ShowInInspector]
        [MinValue(0)]
        public float length {
            get => (end - start).magnitude - buffer.x - buffer.y;
            set => SetLength(value);
        }

        private PrimerBehaviour primer => this.GetPrimer();
        private float realArrowLength => arrowLength * thickness;


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

            if (hasChanges)
                Recalculate();
        }


        public void SwapStartEnd()
        {
            (start, end) = (end, start);
            Recalculate();
        }

        public void Follow(GameObject from, GameObject to)
            => Follow(from.transform, to.transform);

        public void Follow(Transform from, Transform to)
        {
            SetFromTo(from.position, to.position, true);
            startTracker = from;
            endTracker = to;
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

        private void SetLength(float value)
        {
            // If the length is too small, just prevent the change
            if (value < realArrowLength * 2)
                return;

            var diff = end - start;
            end += (value - diff.magnitude) * Vector3.Normalize(diff);
            Recalculate();
        }

        private void Recalculate()
        {
            if (shaft == null || head == null || tail == null || gameObject.IsPreset())
                return;

            shaftLength = length - realArrowLength * 2;

            if (shaftLength <= 0) {
                NoLength();
                return;
            }

            primer.ApplyIntrinsicScale();

            CalculatePosition();
            CalculateChildrenPosition();
        }

        private void CalculatePosition()
        {
            var arrow = transform;
            arrow.rotation = Quaternion.FromToRotation(Vector3.right, end - start);

            if (globalPositioning)
                arrow.position = start;
            else
                arrow.localPosition = start;
        }

        private void CalculateChildrenPosition()
        {
            shaft.localPosition = new Vector3(buffer.x + realArrowLength, 0, 0);
            shaft.localScale = new Vector3(shaftLength, thickness, thickness);

            head.localScale = head.GetPrimer().FindIntrinsicScale() * thickness;
            head.localPosition = new Vector3(shaftLength + buffer.x + realArrowLength, 0, 0);

            tail.localScale = tail.GetPrimer().FindIntrinsicScale() * thickness;
            tail.localPosition = new Vector3(buffer.x + realArrowLength, 0, 0);
        }

        private void NoLength()
        {
            primer.FindIntrinsicScale();
            transform.localScale = Vector3.zero;
        }

        // public void AnimateFromTo(Vector3 from, Vector3 to, float duration = 0.5f, EaseMode ease = EaseMode.Cubic, float endBuffer = 0f, float startBuffer = 0f)
        // {
        //     StartCoroutine(animateFromTo(from, to, duration, ease, endBuffer, startBuffer));
        // }
        // private IEnumerator animateFromTo(Vector3 from, Vector3 to, float duration, EaseMode ease, float endBuffer = 0f, float startBuffer = 0f)
        // {
        //     Vector3 oldPosition = transform.localPosition;
        //     Quaternion oldRotation = transform.localRotation;
        //     float oldLength = currentLength;
        //
        //     //HandleBuffer
        //     float newLength = (from - to).magnitude;
        //     float startBufferFac = startBuffer / newLength;
        //     float endBufferFac = endBuffer / newLength;
        //     Vector3 bFrom = Vector3.Lerp(from, to, startBufferFac);
        //     Vector3 bTo = Vector3.Lerp(to, from, endBufferFac);
        //     newLength -= startBuffer + endBuffer;
        //
        //     //get new rotation
        //     float rads = Mathf.Atan2((bFrom - bTo).y, (bFrom - bTo).x);
        //     Quaternion newRotation = Quaternion.Euler(0, 0, rads * Mathf.Rad2Deg);
        //
        //     float startTime = Time.time;
        //     while (Time.time < startTime + duration)
        //     {
        //         float t = ease.Apply((Time.time - startTime) / duration);
        //         transform.localPosition = Vector3.Lerp(oldPosition, bTo, t);
        //         transform.localRotation = Quaternion.Slerp(oldRotation, newRotation, t);
        //         SetLength(Mathf.Lerp(oldLength, newLength, t));
        //         yield return null;
        //     }
        //     SetFromTo(bFrom, bTo); //Don't include buffer, since 'to' has already been altered
        // }

    }
}
