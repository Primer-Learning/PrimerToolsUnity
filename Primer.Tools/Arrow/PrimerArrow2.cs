using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Tools
{
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

        public Color color = Color.white;

        [Title("Positioning")]
        [InlineButton(nameof(SwapStartEnd))]
        public bool globalPositioning = false;

        public Vector3 start = Vector3.zero;
        public Vector3 end = Vector3.one;
        public Vector2 buffer = Vector2.zero;

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


        #region Unity events
        public void OnValidate() => Recalculate();
        #endregion


        public void SwapStartEnd()
        {
            (start, end) = (end, start);
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
            var diff = end - start;
            var arrow = transform;

            if (globalPositioning) {
                arrow.position = start;
                arrow.rotation = Quaternion.FromToRotation(Vector3.right, diff);
            }
            else {
                arrow.localPosition = start;
                arrow.rotation = Quaternion.FromToRotation(Vector3.right, diff);
            }
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

        private void OnDrawGizmosSelected()
        {
            // Gizmos.DrawCube();
            throw new NotImplementedException();
        }

        // public void SetFromTo(Vector3 from, Vector3 to, float endBuffer = 0f, float startBuffer = 0f,
        //     bool globalPositioning = false)
        // {
        //     var totalLength = (from - to).magnitude;
        //
        //     if (startBuffer + endBuffer >= totalLength) {
        //         NoLength();
        //         return;
        //     }
        //
        //     var startBufferFac = startBuffer / totalLength;
        //     var endBufferFac = endBuffer / totalLength;
        //
        //     var start = Vector3.Lerp(from, to, startBufferFac);
        //     var end = Vector3.Lerp(to, from, endBufferFac);
        //
        //     if (globalPositioning)
        //         transform.position = start;
        //     else
        //         transform.localPosition = end;
        //
        //     length = totalLength - endBuffer - startBuffer;
        //
        //     // //Assume we're looking at the local xy plane. This thing is flat.
        //     // float rads = Mathf.Atan2((start - end).y, (start - end).x);
        //     // transform.localRotation = Quaternion.Euler(0, 0, rads * Mathf.Rad2Deg);
        // }

        private void NoLength()
        {
            primer.FindIntrinsicScale();
            transform.localScale = Vector3.zero;
        }


        //     [Header ("Controls")]
        //     [SerializeField] Vector3 headPosition;
        //     [SerializeField] float headBuffer;
        //     [SerializeField] Vector3 tailPosition = new Vector3(1, 1, 0);
        //     [SerializeField] float tailBuffer;
        //
        //     [Header ("Object and transform references")]
        //     [SerializeField] PrimerObject shaftRoot = null;
        //     [SerializeField] Transform shaft = null;
        //     [SerializeField] Transform head = null;
        //     float currentLength;
        //     public Color Color {
        //         get {
        //             return head.GetComponentsInChildren<MeshRenderer>()[0].materials[0].color;
        //             // Assume head and shaft are the same color
        //         }
        //         set {
        //             head.GetComponentsInChildren<MeshRenderer>()[0].materials[0].color = value;
        //             shaft.GetComponentsInChildren<MeshRenderer>()[0].materials[0].color = value;
        //         }
        //     }
        //
        //
        //     void OnValidate() {
        //         if ( (tailPosition - headPosition).magnitude > 0 ) {
        //             SetFromTo(tailPosition, headPosition, endBuffer: headBuffer, startBuffer: tailBuffer);
        //         }
        //     }
        //
        //     public void SetWidth(float width)
        //     {
        //         transform.localScale = Vector3.one * width;
        //     }
        //
        //     public void SetLength(float length)
        //     {
        //         shaftRoot.transform.localPosition = new Vector3(
        //             length / transform.localScale.x,
        //             shaftRoot.transform.localPosition.y,
        //             shaftRoot.transform.localPosition.z
        //         );
        //         shaft.localScale = new Vector3(
        //             length / transform.localScale.x - 0.1f, //Leave a little room for the head to come to a point
        //             shaftRoot.transform.localScale.y,
        //             shaftRoot.transform.localScale.z
        //         );
        //         head.localPosition = new Vector3(
        //             -length / transform.localScale.x,
        //             shaftRoot.transform.localPosition.y,
        //             shaftRoot.transform.localPosition.z
        //         );
        //         currentLength = length;
        //     }
        //
        //     public void SetFromTo(Vector3 from, Vector3 to, float endBuffer = 0f, float startBuffer = 0f)
        //     {
        //         //HandleBuffer
        //         float length = (from - to).magnitude;
        //         float startBufferFac = startBuffer / length;
        //         float endBufferFac = endBuffer / length;
        //         Vector3 bFrom = Vector3.Lerp(from, to, startBufferFac);
        //         Vector3 bTo = Vector3.Lerp(to, from, endBufferFac);
        //         length -= endBuffer + startBuffer;
        //
        //         //Move object
        //         transform.localPosition = bTo;
        //         SetLength(length);
        //         //Assume we're looking at the local xy plane. This thing is flat.
        //         float rads = Mathf.Atan2((bFrom - bTo).y, (bFrom - bTo).x);
        //         transform.localRotation = Quaternion.Euler(0, 0, rads * Mathf.Rad2Deg);
        //     }
        //
        //     public void AnimateFromTo(Vector3 from, Vector3 to, float duration = 0.5f, EaseMode ease = EaseMode.Cubic, float endBuffer = 0f, float startBuffer = 0f)
        //     {
        //         StartCoroutine(animateFromTo(from, to, duration, ease, endBuffer, startBuffer));
        //     }
        //     private IEnumerator animateFromTo(Vector3 from, Vector3 to, float duration, EaseMode ease, float endBuffer = 0f, float startBuffer = 0f)
        //     {
        //         Vector3 oldPosition = transform.localPosition;
        //         Quaternion oldRotation = transform.localRotation;
        //         float oldLength = currentLength;
        //
        //         //HandleBuffer
        //         float newLength = (from - to).magnitude;
        //         float startBufferFac = startBuffer / newLength;
        //         float endBufferFac = endBuffer / newLength;
        //         Vector3 bFrom = Vector3.Lerp(from, to, startBufferFac);
        //         Vector3 bTo = Vector3.Lerp(to, from, endBufferFac);
        //         newLength -= startBuffer + endBuffer;
        //
        //         //get new rotation
        //         float rads = Mathf.Atan2((bFrom - bTo).y, (bFrom - bTo).x);
        //         Quaternion newRotation = Quaternion.Euler(0, 0, rads * Mathf.Rad2Deg);
        //
        //         float startTime = Time.time;
        //         while (Time.time < startTime + duration)
        //         {
        //             float t = ease.Apply((Time.time - startTime) / duration);
        //             transform.localPosition = Vector3.Lerp(oldPosition, bTo, t);
        //             transform.localRotation = Quaternion.Slerp(oldRotation, newRotation, t);
        //             SetLength(Mathf.Lerp(oldLength, newLength, t));
        //             yield return null;
        //         }
        //         SetFromTo(bFrom, bTo); //Don't include buffer, since 'to' has already been altered
        //     }
        //
        //     public override void ScaleUpFromZero(float duration = 0.5f, EaseMode ease = EaseMode.Cubic, float delay = 0)
        //     {
        //         //Appear from the shaft, even though the point is our center here
        //         shaftRoot.ScaleUpFromZero(duration: duration, ease: ease, delay: delay);
        //     }
        //
        //     public override void SetIntrinsicScale(Vector3 scale)
        //     {
        //         Debug.LogWarning("Arrows are animated by scaling a child object, setting current scale instead of intrinsic scale.");
        //         transform.localScale = scale;
        //     }
        //     public override void SetIntrinsicScale(float scale)
        //     {
        //         SetIntrinsicScale(Vector3.one * scale);
        //     }
        //
        //     public override void SetColor(Color c)
        //     {
        //         Debug.LogWarning("Arrow color has been moved to a property. Just set PrimerArrow.Color normally.");
        //         Color = c;
        //     }

    }
}
