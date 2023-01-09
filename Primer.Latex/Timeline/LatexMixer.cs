using System.Collections.Generic;
using Primer.Animation;
using Primer.Timeline;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Playables;

namespace Primer.Latex
{
    public class LatexMixer : PrimerBoundPlayable<LatexRenderer>
    {
        private LatexTransition currentTransition;
        public AnimationCurve curve = IPrimerAnimation.cubic;
        private LatexTransitionState initial;
        private Transform parent;
        private Vector3 position;

        protected override void Start(LatexRenderer trackTarget)
        {
            parent = trackTarget.transform.parent;
            position = trackTarget.transform.position;
            trackTarget.gameObject.Hide();
        }

        protected override void Stop(LatexRenderer trackTarget)
        {
            trackTarget.gameObject.Show();
            RemoveTransition();
        }

        private void RemoveTransition()
        {
            currentTransition?.Dispose();
            currentTransition = null;
        }

        protected override void Frame(LatexRenderer trackTarget, Playable playable, FrameData info)
        {
            var count = playable.GetInputCount();
            var weights = new List<float>();
            var states = new List<LatexTransitionState>();
            var totalWeight = 0f;

            for (var i = 0; i < count; i++) {
                var weight = playable.GetInputWeight(i);
                var inputPlayable = (ScriptPlayable<PrimerPlayable>)playable.GetInput(i);

                if (weight == 0 || inputPlayable.GetBehaviour() is not LatexTransformerClip.Playable behaviour)
                    continue;

                behaviour.Hide();
                weights.Add(weight);
                states.Add(behaviour.transition);
                totalWeight += weight;
            }

            if (totalWeight == 0) {
                RunStop();
                return;
            }

            RunStart(trackTarget);

            if (states.Count == 1) {
                weights.Insert(0, 1 - totalWeight);
                states.Insert(0, trackTarget.state);
            }

            Assert.IsTrue(states.Count == 2, "LatexMixer can't handle more than two states");

            if (currentTransition is not null && !currentTransition.Is(states[0], states[1]))
                RemoveTransition();

            if (currentTransition is null) {
                currentTransition = new LatexTransition(states[0], states[1], curve);
                currentTransition.Place(parent, position);
            }

            currentTransition.Apply(weights[1]);
        }
    }
}
