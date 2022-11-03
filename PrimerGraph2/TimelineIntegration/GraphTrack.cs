using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Graph
{
    [DisplayName("Primer / Graph Track")]
    [TrackClipType(typeof(PlotCurveClip))]
    [TrackBindingType(typeof(Graph2))]
    public class GraphTrack : TrackAsset
    {
        protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip) {
            Debug.Log("CREATE PLAYABLE");
            var playable = base.CreatePlayable(graph, gameObject, clip);

            var behaviour = ((ScriptPlayable<PlotCurveBehaviour>)playable).GetBehaviour();

            if (behaviour is not null) {
                behaviour.duration = clip.duration;
            }

            return playable;
        }
    }
}
