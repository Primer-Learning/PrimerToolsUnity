using Primer;
using Primer.Animation;
using UnityEngine;

[RequireComponent(typeof(PrimerBlob))]
public class Blob : MonoBehaviour
{
    private static readonly int scoop = Animator.StringToHash("Scoop");

    private PrimerBlob blobCache;
    public PrimerBlob blob => this.GetOrAddComponent(ref blobCache);

    private Animator animatorCache;
    public Animator animator => this.GetOrAddComponent(ref animatorCache);

    public Vector3 mouth => transform.TransformPoint(0.04f, 1.25f, 0.45f);

    public void Scoop()
    {
        animator.SetTrigger(scoop);
    }

    public Tween TweenLookAt(Transform camTransform)
    {
        var t = transform;
        var initial = t.rotation;
        t.LookAt(camTransform);
        var target = t.rotation;
        t.rotation = initial;
        return t.RotateTo(target, initial);
    }
}
