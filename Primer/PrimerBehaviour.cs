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

        public void CancelOnDestroy(CancellationTokenSource tokenSource)
        {
            lifetime.Register(tokenSource.Cancel);
        }


        #region Vector3? intrinsicScale
        [SerializeField]
        [Title("Intrinsic scale")]
        [InlineButton(nameof(IntrinsicScaleIsCurrentScale), SdfIconType.ArrowDown, "From transform")]
        [InlineButton(
            nameof(ApplyIntrinsicScale),
            SdfIconType.ArrowUp,
            "To transform",
            ShowIf = nameof(hasIntrinsicScale)
        )]
        private bool hasIntrinsicScale = false;

        [SerializeField]
        [EnableIf(nameof(hasIntrinsicScale))]
        private Vector3 intrinsicScale = Vector3.one;

        public void IntrinsicScaleIsCurrentScale() => SetIntrinsicScale(transform.localScale);

        public void SetIntrinsicScale(Vector3 value)
        {
            hasIntrinsicScale = true;
            intrinsicScale = value;
        }

        public void ApplyIntrinsicScale()
        {
            transform.localScale = FindIntrinsicScale();
        }

        /// <summary>
        ///     This is at the same time a getter for intrinsic scale and also finds and sets it if it's not defined
        /// </summary>
        public Vector3 FindIntrinsicScale()
        {
            if (hasIntrinsicScale)
                return intrinsicScale;

            // We don't have intrinsic scale but the transform's scale is 0...
            // so we don't save it
            if (transform.localScale == Vector3.zero)
                return Vector3.zero;

            var scale = transform.localScale;
            SetIntrinsicScale(scale);
            return scale;
        }
        #endregion

        #region Vector3? intrinsicPosition
        [SerializeField]
        [Title("Intrinsic position")]
        [InlineButton(nameof(IntrinsicPositionIsCurrentPosition), SdfIconType.ArrowDown, "From transform")]
        [InlineButton(
            nameof(ApplyIntrinsicPosition),
            SdfIconType.ArrowUp,
            "To transform",
            ShowIf = nameof(hasIntrinsicPosition)
        )]
        private bool hasIntrinsicPosition = false;

        [SerializeField]
        [EnableIf(nameof(hasIntrinsicPosition))]
        private Vector3 intrinsicPosition = Vector3.zero;

        public void IntrinsicPositionIsCurrentPosition() => SetIntrinsicPosition(transform.localPosition);

        public void SetIntrinsicPosition(Vector3 value)
        {
            hasIntrinsicPosition = true;
            intrinsicPosition = value;
        }

        public void ApplyIntrinsicPosition()
        {
            transform.localPosition = FindIntrinsicPosition();
        }

        public Vector3 FindIntrinsicPosition()
        {
            if (hasIntrinsicPosition)
                return intrinsicPosition;

            var position = transform.localPosition;
            SetIntrinsicPosition(position);
            return position;
        }
        #endregion;
    }
}
