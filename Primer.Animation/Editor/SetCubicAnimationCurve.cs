using JetBrains.Annotations;
using UnityEditor.Timeline.Actions;
using UnityEngine;

namespace Primer.Animation.Editor
{
    [UsedImplicitly]
    [MenuEntry("Primer / Set cubic curves on recorded clips", priority: 9000)]
    public class SetCubicAnimationCurve : ForceCubicAnimationCurve
    {
        protected override void ChangeAnimationCurves(AnimationClip clip)
        {
            if (clip.IsRecordedAndPristine())
                base.ChangeAnimationCurves(clip);
        }
    }
}
