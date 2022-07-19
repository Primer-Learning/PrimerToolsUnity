using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace LatexRenderer.Timeline
{
    public class LatexTransitionClip : PlayableAsset, ITimelineClipAsset
    {
        public ExposedReference<Transform> before;
        public ExposedReference<Transform> after;

        public ExposedReference<Transform> beforeAnchor;
        public ExposedReference<Transform> afterAnchor;

        public List<MorphTransition> morphTransitions;
        public ClipCaps clipCaps => ClipCaps.None;


        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var transitionPlayable = ScriptPlayable<LatexTransitionBehaviour>.Create(graph);
            var transition = transitionPlayable.GetBehaviour();

            var resolver = graph.GetResolver();
            transition.After = after.Resolve(resolver);
            transition.AfterAnchor = afterAnchor.Resolve(resolver);

            transition.Before = before.Resolve(resolver);
            transition.BeforeAnchor = beforeAnchor.Resolve(resolver);

            transition.MorphTransitions = morphTransitions.Select(i =>
                    (i.beforeChild.Resolve(resolver), i.afterChild.Resolve(resolver)))
                .Where(i => i.Item1 && i.Item2).ToList();

            return transitionPlayable;
        }

        [Serializable]
        public class MorphTransition
        {
            public ExposedReference<Transform> beforeChild;
            public ExposedReference<Transform> afterChild;
        }
    }
}