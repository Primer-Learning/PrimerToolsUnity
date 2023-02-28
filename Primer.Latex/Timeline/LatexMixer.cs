using System;
using System.Collections.Generic;
using Primer.Animation;
using Primer.Latex.FakeUnityEngine;
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
            if (trackTarget == null)
                throw new Exception($"{nameof(LatexTrack)}'s needs to be bound to a {nameof(LatexRenderer)}");

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

        private LatexTransitionState ProcessPlayable(LatexTransformerClip.Playable playable)
        {
            var state = playable.state;
            state.transform.gameObject.Hide();
            return state;
        }

        private void RemoveState()
        {
            if (currentState is null)
                return;

            if (currentState.transform != trackTarget.transform)
                currentState.Restore();

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

            snapshot.ApplyTo(state.transform, offsetPosition: initialState.GetOffsetWith(state));
            currentState = state;
        }

        private void Transition(LatexTransitionState state1, LatexTransitionState state2, float t)
        {
            RemoveState();

            if (currentTransition is not null && !currentTransition.Is(state1, state2))
                RemoveTransition();

            if (currentTransition is null) {
                currentTransition = new LatexTransition(state1, state2, curve);
                snapshot.ApplyTo(currentTransition.transform, offsetPosition: initialState.GetOffsetWith(state1));
            }

            currentTransition.Apply(t);
        }
    }
}
