using System;
using System.Collections.Generic;
using Primer.Animation;
using Primer.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Primer.Latex
{
    public class LatexMixer : PrimerBoundPlayable<LatexRenderer>
    {
        private GameObject applied;
        public AnimationCurve curve = IPrimerAnimation.cubic;
        private GameObject original;
        private GameObject transformable;

        protected override void Start(LatexRenderer trackTarget)
        {
            original = trackTarget.gameObject;
            RestoreOriginal();
            original.Hide();
        }

        protected override void Stop(LatexRenderer trackTarget)
        {
            transformable.Dispose(urgent: true);
            original.Show();
            applied = null;
        }

        private List<Transform> RestoreOriginal()
        {
            if (applied == original)
                return null;

            transformable.Dispose();
            transformable = Object.Instantiate(original);
            transformable.Show();
            original.Hide();
            applied = original;
            return null;
        }

        private void DisableBehaviour(LatexTransformer behaviour)
        {
            if (behaviour.transformTo)
                behaviour.transformTo.gameObject.Hide();
        }

        private List<Transform> ApplyState(LatexTransformer behaviour)
        {
            if (behaviour.transformTo == null)
                return null;

            var target = behaviour.transformTo.transform;

            if (applied == target.gameObject)
                return null;

            var modifier = new ChildrenModifier(transformable.transform);
            var originalCursor = -1;
            var behaviourCursor = -1;

            foreach (var transition in behaviour.transitions) {
                originalCursor++;

                if (transition == TransitionType.Remove)
                    continue;

                behaviourCursor++;

                var child = transition == TransitionType.Replace
                    ? target.GetChild(behaviourCursor)
                    : original.transform.GetChild(originalCursor);

                modifier.NextMustBe(Object.Instantiate(child));
            }

            applied = target.gameObject;
            return modifier.Apply();
        }

        protected override void Frame(LatexRenderer trackTarget, Playable playable, FrameData info)
        {
            var count = playable.GetInputCount();
            var weights = new List<float>();
            var behaviours = new List<LatexTransformer>();
            var totalWeight = 0f;

            for (var i = 0; i < count; i++) {
                var weight = playable.GetInputWeight(i);

                var inputPlayable = (ScriptPlayable<PrimerPlayable>)playable.GetInput(i);

                if (inputPlayable.GetBehaviour() is not LatexTransformer behaviour)
                    continue;

                if (weight == 0) {
                    DisableBehaviour(behaviour);
                    continue;
                }

                if (weight >= 1) {
                    ApplyState(behaviour);
                    return;
                }

                weights.Add(weight);
                behaviours.Add(behaviour);
                totalWeight += weight;
            }

            if (totalWeight == 0) {
                RunStop();
                return;
            }

            RunStart(trackTarget);

            if (totalWeight >= 1)
                MixStates(weights, behaviours);
            else
                MixStatesWithOriginal(behaviours[0], weights[0]);
        }

        private void MixStatesWithOriginal(LatexTransformer behaviour, float weight)
        {
            if (!behaviour.transformTo) {
                RestoreOriginal();
                return;
            }

            var originalCursor = 0;
            var behaviourCursor = 0;
            var transformableCursor = 0;
            var cubic = curve.Evaluate(weight);
            var isFirstHalf = weight < 0.5f;

            var children =
                (isFirstHalf ? RestoreOriginal() : ApplyState(behaviour))
                ?? transformable.transform.GetChildren();

            foreach (var transition in behaviour.transitions) {
                if (!isFirstHalf && (transition == TransitionType.Remove)) {
                    originalCursor++;
                    continue;
                }

                var originalGroup = original.transform.GetChild(originalCursor++);
                var behaviourGroup = behaviour.transformTo.transform.GetChild(behaviourCursor++);
                var transformableGroup = children[transformableCursor++];

                transformableGroup.localPosition = Vector3.Lerp(
                    originalGroup.localPosition,
                    behaviourGroup.localPosition,
                    cubic
                );

                if (transition == TransitionType.Replace) {
                    var scale = isFirstHalf ? originalGroup.localScale : behaviourGroup.localScale;
                    transformableGroup.localScale = Mathf.Abs((cubic - 0.5f) * 2) * scale;
                }
            }
        }

        private void MixStates(List<float> weights, List<LatexTransformer> behaviours)
        {
            // Debug.Log("MixBehaviours");
            throw new NotImplementedException();
        }
    }
}
