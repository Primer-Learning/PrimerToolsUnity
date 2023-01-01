using System;
using System.Collections.Generic;
using Primer.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Primer.Latex
{
    public class LatexMixer : PrimerBoundPlayable<LatexRenderer>
    {
        private GameObject applied;
        private GameObject original;
        private GameObject transformable;

        protected override void Start(LatexRenderer trackTarget)
        {
            Debug.Log("START");
            original = trackTarget.gameObject;
            RestoreOriginal();
            original.SetActive(false);
        }

        protected override void Stop(LatexRenderer trackTarget)
        {
            Debug.Log("STOP");
            transformable.Dispose();
            original.SetActive(true);
            applied = null;
        }

        private void RestoreOriginal()
        {
            if (applied == original)
                return;

            Debug.Log("RestoreOriginal");
            transformable.Dispose();
            transformable = Object.Instantiate(original);
            transformable.SetActive(true);
            applied = original;
        }

        private void ApplyBehaviour(GroupTransformer behaviour)
        {
            if (behaviour.transformTo == null)
                return;

            var target = behaviour.transformTo.transform;

            if (applied == target.gameObject)
                return;

            Debug.Log("ApplyBehaviour");
            var modifier = new ChildrenModifier(target);
            var orig = original.transform;

            var originalCursor = -1;
            var behaviourCursor = -1;

            foreach (var transition in behaviour.transitions) {
                originalCursor++;

                if (transition == TransitionType.Remove)
                    continue;

                behaviourCursor++;

                Debug.Log($"{behaviourCursor} - {originalCursor}");

                var child = transition == TransitionType.Replace
                    ? target.GetChild(behaviourCursor)
                    : orig.GetChild(originalCursor);

                modifier.NextMustBe(child);
            }

            modifier.Apply();

            // transformable.Dispose();
            // transformable = Object.Instantiate(target);
            // transformable.transform.localPosition = original.transform.localPosition;
            // transformable.SetActive(true);
            // applied = target;
        }

        protected override void Frame(LatexRenderer trackTarget, Playable playable, FrameData info)
        {
            var count = playable.GetInputCount();

            var weights = new List<float>();
            var behaviours = new List<GroupTransformer>();
            var totalWeight = 0f;

            for (var i = 0; i < count; i++) {
                var weight = playable.GetInputWeight(i);

                var inputPlayable = (ScriptPlayable<PrimerPlayable>)playable.GetInput(i);

                if (inputPlayable.GetBehaviour() is not GroupTransformer behaviour)
                    continue;

                switch (weight) {
                    case 0:
                        DisableBehaviour(behaviour);
                        continue;
                    case >= 1:
                        ApplyBehaviour(behaviour);
                        return;
                }

                weights.Add(weight);
                behaviours.Add(behaviour);
                totalWeight += weight;
            }

            switch (totalWeight) {
                case 0:
                    RunStop();
                    return;
                case >= 1:
                    MixBehaviours(weights, behaviours);
                    return;
                default: {
                    MixBehaviourWithOriginal(behaviours[0], weights[0]);
                    break;
                }
            }
        }

        private void DisableBehaviour(GroupTransformer behaviour)
        {
            Debug.Log("DisableBehaviour");

            if (behaviour.transformTo != null)
                behaviour.transformTo.gameObject.SetActive(false);
        }

        private void MixBehaviourWithOriginal(GroupTransformer behaviour, float weight)
        {
            Debug.Log("MixBehaviourWithOriginal");
            var transitions = behaviour.transitions;
            var disappearWeight = 1 - Mathf.Clamp(weight * 2, 0, 1);
            var appearWeight = Mathf.Clamp(weight * 2 - 1, 0, 1);
            var isFirstHalf = weight < 0.5f;

            if (isFirstHalf)
                RestoreOriginal();
            else
                ApplyBehaviour(behaviour);

            var originalTransform = original.transform;
            var behaviourTransform = behaviour.transformTo.transform;
            var transformableTransform = transformable.transform;

            var originalCursor = 0;
            var behaviourCursor = 0;
            var transformableCursor = 0;

            foreach (var transition in transitions) {
                if (!isFirstHalf && (transition == TransitionType.Remove)) {
                    originalCursor++;
                    continue;
                }

                var originalGroup = originalTransform.GetChild(originalCursor++);
                var behaviourGroup = behaviourTransform.GetChild(behaviourCursor++);
                var transformableGroup = transformableTransform.GetChild(transformableCursor++);

                transformableGroup.localPosition = Vector3.Lerp(
                    originalGroup.localPosition,
                    behaviourGroup.localPosition,
                    weight
                );

                if (transition == TransitionType.Replace) {
                    transformableGroup.localScale = isFirstHalf
                        ? originalGroup.localScale * disappearWeight
                        : behaviourGroup.localScale * appearWeight;
                }
            }
        }

        private void MixBehaviours(List<float> weights, List<GroupTransformer> behaviours)
        {
            Debug.Log("MixBehaviours");
            throw new NotImplementedException();
        }
    }
}
