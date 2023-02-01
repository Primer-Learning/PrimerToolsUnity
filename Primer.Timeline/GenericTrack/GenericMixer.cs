using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline.GenericTrack
{
    // For this track in particular we skip the PrimerBehaviour and just use PlayableBehaviour
    //  because this is a special case where we want to execute past clips too.
    public class GenericMixer : PlayableBehaviour
    {
        private readonly List<GenericBehaviour> activeClips = new();

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var time = (float)playable.GetTime();
            var behaviours = CollectBehaviours(playable)
                .Where(x => x.start < time)
                .ToArray();

            foreach (var behaviour in FindBehavioursToExecute(behaviours)) {
                behaviour.Execute(time);

                if (behaviour.end < time)
                    activeClips.Add(behaviour);
            }
        }

        private IEnumerable<GenericBehaviour> FindBehavioursToExecute(IReadOnlyList<GenericBehaviour> behaviours)
        {
            var requireCompleteExecution = behaviours.Count < activeClips.Count;
            var result = new List<GenericBehaviour>();

            for (var i = 0; i < behaviours.Count; i++) {
                var clip = behaviours[i];

                // this is a new clip
                if (i >= activeClips.Count) {
                    result.Add(clip);
                    continue;
                }

                // this clip has been executed in the right order
                if (clip.Equals(activeClips[i]))
                    continue;

                requireCompleteExecution = true;
                break;
            }

            if (!requireCompleteExecution)
                return result;

            activeClips.Clear();
            return behaviours;

        }

        private static IEnumerable<GenericBehaviour> CollectBehaviours(Playable playable)
        {
            var behaviours = new List<GenericBehaviour>();

            for (var i = 0; i < playable.GetInputCount(); i++) {
                var inputPlayable = (ScriptPlayable<GenericBehaviour>)playable.GetInput(i);

                if (inputPlayable.GetBehaviour() is {} behaviour)
                    behaviours.Add(behaviour);
            }

            behaviours.Sort(new PlayableTimeComparer());
            return behaviours;
        }

        public override object Clone()
        {
            Debug.Log($"Clone");
            return base.Clone();
        }
    }

    public class PlayableTimeComparer : IComparer<GenericBehaviour>
    {
        public int Compare(GenericBehaviour left, GenericBehaviour right)
        {
            if (left is null && right is null)
                return 0;

            if (left is null)
                return 1;

            if (right is null)
                return -1;

            var delta = left.start - right.start;
            return (int) (delta * 10000);
        }
    }
}
