using System.ComponentModel;
using Primer.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Graph
{
    [DisplayName("Primer / Graph Point Track")]
    [TrackClipType(typeof(PlottedEquationClip))]
    [TrackClipType(typeof(PlottedDataClip))]
    [TrackBindingType(typeof(GraphPoint))]
    public class GraphPointTrack : PrimerTrack
    {
        [SerializeReference]
        public PrimerAnimator fadeIn = new ScaleUpAndTweenYAnimator();

        [SerializeReference]
        public PrimerAnimator fadeOut = new ScaleUpFromZeroAnimator();

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) {
            var mixer = ScriptPlayable<GraphPointMixer>.Create(graph, inputCount);
            mixer.GetBehaviour().getAppearanceAnimator = isFadeOut => isFadeOut ? fadeOut : fadeIn;
            return mixer;
        }
    }

}
