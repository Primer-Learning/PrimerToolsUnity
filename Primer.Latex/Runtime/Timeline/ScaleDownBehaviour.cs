using UnityEngine;
using UnityEngine.Playables;

namespace LatexRenderer.Timeline
{
    public class ScaleDownBehaviour : PlayableBehaviour
    {
        private Vector3? _originalScale;
        public Transform Target;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            Target.localScale = Vector3.Lerp(_originalScale.Value, Vector3.zero,
                (float)(playable.GetTime() / playable.GetDuration()));
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (_originalScale.HasValue) Target.localScale = _originalScale.Value;
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            _originalScale = Target.localScale;
        }
    }
}