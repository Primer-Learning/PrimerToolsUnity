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
        [Tooltip(
            "Parent object containing all the released SVG parts that are visible *before* the transition.")]
        public ExposedReference<Transform> before;

        [Tooltip(
            "Parent object containing all the released SVG parts that are visible *after* the transition.")]
        public ExposedReference<Transform> after;

        [Tooltip("Child of `before` that will be used for alignment: see `afterAnchor`.")]
        public ExposedReference<Transform> beforeAnchor;

        [Tooltip(
            "Child of `after`. At the start of the transition, `after` will be moved such that the center of `afterAnchor` lines up exactly with the center of `beforeAnchor`. This allows you to keep the two expressions separated during editing.")]
        public ExposedReference<Transform> afterAnchor;

        public List<Transition> morphTransitions;
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
        public class Transition
        {
            public ExposedReference<Transform> beforeChild;
            public ExposedReference<Transform> afterChild;
        }
    }
}