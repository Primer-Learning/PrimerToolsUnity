using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace LatexRenderer.Timeline
{
    public class ScaleDownBehaviour : PlayableBehaviour
    {
        private List<Target> _targets;

        public void SetTargets(IEnumerable<Transform> targets)
        {
            _targets = targets.Select(i => new Target { Transform = i }).ToList();
        }


        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            foreach (var i in _targets)
                i.Transform.localScale = Vector3.Lerp(i.OriginalScale.Value, Vector3.zero,
                    (float)(playable.GetTime() / playable.GetDuration()));
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            foreach (var i in _targets)
                if (i.OriginalScale.HasValue)
                    i.Transform.localScale = i.OriginalScale.Value;
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            foreach (var i in _targets) i.OriginalScale = i.Transform.localScale;
        }

        private class Target
        {
            public Vector3? OriginalScale;
            public Transform Transform;
        }
    }
}