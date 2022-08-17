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

        [Tooltip("Child of Before that will be used for alignment: see After Anchor.")]
        public ExposedReference<Transform> beforeAnchor;

        [Tooltip(
            "Child of After. At the start of the transition, After will be moved such that the center of After Anchor lines up exactly with the center of Before Anchor. This allows you to keep the two expressions separated during editing.")]
        public ExposedReference<Transform> afterAnchor;

        [Tooltip(
            "Transitions between two objects that only differ in position and/or scale. The Before Child will be moved such that its center lines up exactly with After Child's center. The Before Child will also be scaled until it's exactly the same width as After Child. Then, simultaneously, the After Child will be shown and the Before Child hidden.")]
        public List<Transition> morphTransitions = new();

        [Tooltip(
            "Transitions between two dissimilar objects. The Before Child will be scaled down to zero. The Before Child will also be moved such that its center lines up exactly with After Child's center. The After Child will then be scaled up from zero.")]
        public List<Transition> scaleDownAndMoveTransitions = new();

        public ClipCaps clipCaps => ClipCaps.None;


        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var transitionPlayable = ScriptPlayable<LatexTransitionBehaviour>.Create(graph);
            var transition = transitionPlayable.GetBehaviour();
            UpdateLatexTransitionBehaviour(graph.GetResolver(), transition);

            return transition.GetValidationErrors().Count > 0 ? Playable.Null : transitionPlayable;
        }

        private void UpdateLatexTransitionBehaviour(IExposedPropertyTable resolver,
            LatexTransitionBehaviour transition)
        {
            transition.After = after.Resolve(resolver);
            transition.AfterAnchor = afterAnchor.Resolve(resolver);

            transition.Before = before.Resolve(resolver);
            transition.BeforeAnchor = beforeAnchor.Resolve(resolver);

            transition.MorphTransitions = morphTransitions.Select(i =>
                (i.beforeChild.Resolve(resolver), i.afterChild.Resolve(resolver))).ToList();
            transition.ScaleDownAndMoveTransitions = scaleDownAndMoveTransitions.Select(i =>
                (i.beforeChild.Resolve(resolver), i.afterChild.Resolve(resolver))).ToList();
        }

        public List<string> GetValidationErrors(IExposedPropertyTable resolver)
        {
            LatexTransitionBehaviour transition = new();
            UpdateLatexTransitionBehaviour(resolver, transition);
            return transition.GetValidationErrors();
        }

        [Serializable]
        public class Transition
        {
            [Tooltip("Child of Before that will transition into After Child.")]
            public ExposedReference<Transform> beforeChild;

            [Tooltip("Child of After.")] public ExposedReference<Transform> afterChild;
        }
    }
}