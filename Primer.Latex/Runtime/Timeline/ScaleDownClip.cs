using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace LatexRenderer.Timeline
{
    public class ScaleDownClip : PlayableAsset, ITimelineClipAsset
    {
        public ExposedReference<Transform> target;
        public ClipCaps clipCaps => ClipCaps.None;


        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var scaleDown = ScriptPlayable<ScaleDownBehaviour>.Create(graph);
            scaleDown.GetBehaviour().Target = target.Resolve(graph.GetResolver());
            return scaleDown;

            // AnimationClip clip = new();
            // clip.SetCurve("Svg Part 0", typeof(Transform), "localScale/x",
            //     AnimationCurve.Linear(0, 1, 10, 0));
            //
            // return AnimationClipPlayable.Create(graph, clip);
        }
    }
}