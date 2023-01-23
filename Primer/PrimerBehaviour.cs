using System.Threading;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer
{
    [AddComponentMenu("Primer / Primer Behaviour")]
    public class PrimerBehaviour : MonoBehaviour
    {
        private readonly CancellationTokenSource lifetimeCancellation = new();

        public CancellationToken lifetime => lifetimeCancellation.Token;


        private void OnDestroy()
        {
            lifetimeCancellation.Cancel();
        }


        #region Vector3? intrinsicScale
        [SerializeField]
        [Title("Intrinsic scale")]
        private bool hasIntrinsicScale = false;

        [SerializeField]
        [EnableIf(nameof(hasIntrinsicScale))]
        private Vector3 intrinsicScale = Vector3.one;

        public void SetIntrinsicScale(Vector3 value)
        {
            hasIntrinsicScale = true;
            intrinsicScale = value;
        }

        public Vector3 FindIntrinsicScale()
        {
            if (hasIntrinsicScale)
                return intrinsicScale;

            // We don't have intrinsic scale but the transform's scale is 0...
            // but we don't save it
            if (transform.localScale == Vector3.zero)
                return Vector3.zero;

            var scale = transform.localScale;
            SetIntrinsicScale(scale);
            return scale;
        }

        public void ApplyIntrinsicScale()
        {
            transform.localScale = FindIntrinsicScale();
        }
        #endregion

        #region Vector3? intrinsicPosition
        [SerializeField]
        [Title("Intrinsic position")]
        private bool hasIntrinsicPosition = false;

        [SerializeField]
        [EnableIf(nameof(hasIntrinsicPosition))]
        private Vector3 intrinsicPosition = Vector3.one;

        public void SetIntrinsicPosition(Vector3 value)
        {
            hasIntrinsicPosition = true;
            intrinsicPosition = value;
        }

        public Vector3 FindIntrinsicPosition()
        {
            if (hasIntrinsicPosition)
                return intrinsicPosition;

            // We don't have intrinsic scale but the transform's scale is 0...
            // but we don't save it
            if (transform.localPosition == Vector3.zero)
                return Vector3.zero;

            var scale = transform.localPosition;
            SetIntrinsicPosition(scale);
            return scale;
        }

        public void ApplyIntrinsicPosition()
        {
            transform.localPosition = FindIntrinsicPosition();
        }
        #endregion;
    }
}
