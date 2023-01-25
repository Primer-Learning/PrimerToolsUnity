using Primer.Animation;
using Primer.Timeline;
using UnityEngine;
using UnityEngine.Assertions;

namespace Primer.Latex
{
    internal class LatexMixer : PrimerMixer<LatexRenderer, LatexTransitionState>
    {
        private LatexTransitionState currentState;
        private LatexTransition currentTransition;
        private TransformSnapshot snapshot;

        public AnimationCurve curve = IPrimerAnimation.cubic;

        public LatexTransitionState initialState => trackTarget.state;


        protected override void Start()
        {
            snapshot = new TransformSnapshot(trackTarget.transform);
            trackTarget.gameObject.Hide();
        }

        protected override void Stop()
        {
            trackTarget.gameObject.Show();
            snapshot = null;
            RemoveTransition();
            RemoveState();
        }

        private static LatexTransitionState ProcessPlayable(LatexTransformerClip.Playable playable)
        {
            var state = playable.state;
            state.transform.gameObject.Hide();
            return state;
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


        protected override IMixerCollector<LatexTransitionState> CreateCollector()
            => new MixerCollector<LatexTransformerClip.Playable, LatexTransitionState>(ProcessPlayable);

        protected override void Frame(IMixerCollector<LatexTransitionState> collector)
        {
            if (collector.count == 1) {
                if (collector.isFull) {
                    ApplyState(collector[0].input);
                    return;
                }

                collector.AddInitialState(initialState);
            }

            Assert.AreEqual(collector.count, 2, "LatexMixer can't handle more than two states");
            Transition(collector[0].input, collector[1].input, collector[1].weight);
        }


        private void ApplyState(LatexTransitionState state)
        {
            RemoveTransition();

            if (currentState is not null && (currentState != state))
                RemoveState();

            snapshot.ApplyTo(state.transform);
            state.transform.localPosition += initialState.GetOffsetWith(state);
            currentState = state;
        }

        private void Transition(LatexTransitionState state1, LatexTransitionState state2, float t)
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
    }
}
