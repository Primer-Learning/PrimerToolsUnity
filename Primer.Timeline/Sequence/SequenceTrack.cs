using UnityEngine.Playables;
using UnityEngine.Timeline;

// ReSharper disable once CheckNamespace
// Namespace does not correspond to file location, should be: 'Primer.Timeline'
// We use FakeUnityEngine namespace because if "UnityEngine" is part of the namespace Unity allow us
//  to show this track without submenu
namespace Primer.Timeline.FakeUnityEngine
{
    [TrackClipType(typeof(SequenceClip))]
    [TrackBindingType(typeof(Sequence))]
    public class SequenceTrack : PrimerTrack
    {
        protected override float defaultDuration { get; } = 0.5f;
        protected override Playable CreateMixer(PlayableGraph graph, int inputCount)
        {
            return ScriptPlayable<SequenceMixer>.Create(graph, inputCount);
        }
    }
}
