using System.Threading;
using Cysharp.Threading.Tasks;
using Primer;
using Primer.Extensions;
using UnityEngine;

[AddComponentMenu("Primer Learning / Primer Behaviour")]
public class PrimerBehaviour : MonoBehaviour
{
    #region Lifetime cancellation token
    private readonly CancellationTokenSource lifetimeCancellation = new();
    private CancellationToken lifetime => lifetimeCancellation.Token;

    private void OnDestroy() => lifetimeCancellation.Cancel();
    #endregion


    #region Intrinsic scale
    private Vector3? intrinsicScaleNullable;
    public Vector3 intrinsicScale {
        get => ReadIntrinsicScale();
        set => intrinsicScaleNullable = value;
    }

    public Vector3 ReadIntrinsicScale()
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
    #endregion


    public async UniTask ScaleUpFromZero(PrimerAnimation anim = null)
    {
        ReadIntrinsicScale();
        transform.localScale = Vector3.zero;
        await transform.ScaleTo(intrinsicScale, anim, lifetime);
    }

    public async UniTask ScaleDownToZero(PrimerAnimation anim = null)
    {
        ReadIntrinsicScale();
        await transform.ScaleTo(Vector3.zero, anim, lifetime);
    }

    public async void ShrinkAndDispose(PrimerAnimation anim = null) {
        if (Application.isPlaying)
            await ScaleDownToZero(anim);

        // This is false if the element has already been destroyed
        if (this)
            gameObject.Dispose();
    }
}
