using System.Threading;
using UnityEngine;

namespace Primer
{
    [AddComponentMenu("Primer / Primer Behaviour")]
    public class PrimerBehaviour : MonoBehaviour
    {
        private readonly CancellationTokenSource lifetimeCancellation = new();
        private Vector3? intrinsicScaleNullable;
        public CancellationToken lifetime => lifetimeCancellation.Token;

        private void OnDestroy()
        {
            lifetimeCancellation.Cancel();
        }

        #region Intrinsic scale
        public Vector3 intrinsicScale {
            get => SaveIntrinsicScale();
            set => intrinsicScaleNullable = value;
        }

        public Vector3 SaveIntrinsicScale()
        {
            if (intrinsicScaleNullable.HasValue)
                return intrinsicScaleNullable.Value;

            // We don't have intrinsic scale but the transform's scale is 0...
            if (transform.localScale == Vector3.zero)
                return Vector3.zero;

            var scale = transform.localScale;
            intrinsicScaleNullable = scale;
            return scale;
        }

        public void RestoreIntrinsicScale()
        {
            transform.localScale = intrinsicScale;
        }
        #endregion
    }
}
