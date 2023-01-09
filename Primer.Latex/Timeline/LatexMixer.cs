using System.Linq;
using Primer.Animation;
using Primer.Timeline;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Playables;

namespace Primer.Latex
{
    public class LatexMixer : PrimerBoundPlayable<LatexRenderer>
    {
        private LatexTransitionState currentState;
        private LatexTransition currentTransition;
        public AnimationCurve curve = IPrimerAnimation.cubic;
        private TransformSnapshot snapshot;

        protected override void Start(LatexRenderer trackTarget)
        {
            snapshot = trackTarget.GetOrAddComponent<TransformSnapshot>();
            trackTarget.gameObject.Hide();
        }

        protected override void Stop(LatexRenderer trackTarget)
        {
            trackTarget.gameObject.Show();
            RemoveTransition();
            RemoveState();
        }

        private void RemoveState()
        {
            currentState?.Restore();
            currentState = null;
        }

        private void RemoveTransition()
        {
            currentTransition?.Dispose();
            currentTransition = null;
        }

        protected override void Frame(LatexRenderer trackTarget, Playable playable, FrameData info)
        {
            var (weights, behaviours) = CollectInputs<LatexTransformerClip.Playable>(playable);
            var states = behaviours.Select(x => x.state).ToList();
            var totalWeight = weights.Sum();

            if (totalWeight == 0) {
                RunStop();
                return;
            }

            RunStart(trackTarget);

            if (states.Count == 1) {
                if (weights[0] >= 1) {
                    ApplyState(trackTarget.state, states[0]);
                    return;
                }

                weights.Insert(0, 1 - totalWeight);
                states.Insert(0, trackTarget.state);
            }

            Assert.IsTrue(states.Count == 2, "LatexMixer can't handle more than two states");
            Transition(trackTarget.state, states[0], states[1], weights[1]);
        }

        private void Transition(LatexTransitionState initialState, LatexTransitionState state1,
            LatexTransitionState state2, float t)
        {
            RemoveState();

            if (currentTransition is not null && !currentTransition.Is(state1, state2))
                RemoveTransition();

            if (currentTransition is null) {
                currentTransition = new LatexTransition(state1, state2, curve);
                snapshot.ApplyTo(currentTransition.transform);
                currentTransition.transform.localPosition += initialState.GetOffsetWith(state1);
            }

            currentTransition.Apply(t);
        }

        private void ApplyState(LatexTransitionState initial, LatexTransitionState state)
        {
            RemoveTransition();

            if (currentState is not null && (currentState != state))
                RemoveState();

            if (currentState is not null)
                return;

            snapshot.ApplyTo(state.transform);
            state.transform.localPosition += initial.GetOffsetWith(state);
            currentState = state;
        }
    }
}
