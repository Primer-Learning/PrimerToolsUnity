using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Primer.Animation;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Primer.Latex
{
    [Serializable]
    public class LatexScrubbable : Scrubbable, IDisposable
    {
        #region Inspector
        [SerializeReference]
        [HideInInspector]
        private LatexRenderer _from;

        [ShowInInspector]
        public LatexRenderer from {
            get => _from;
            set {
                _from = value;
                transition = null;
                UpdateGroupsLength();
            }
        }

        [SerializeReference]
        [HideInInspector]
        private LatexRenderer _to;

        [ShowInInspector]
        public LatexRenderer to {
            get => _to;
            set {
                _to = value;
                transition = null;
                UpdateGroupsLength();
            }
        }

        [SerializeReference]
        [HideInInspector]
        private List<TransitionType> _groups = new();

        [ShowInInspector]
        public List<TransitionType> groups {
            get => _groups;
            set {
                _groups = value ?? new List<TransitionType>();
                transition = null;
                UpdateGroupsLength();
            }
        }
        #endregion

        #region private LatexTransition transition { get; set; }
        private LatexTransition _transition;
        private LatexTransition transition {
            get => EnsureTransitionExists();
            set => ResetTransition(value);
        }

        private LatexTransition EnsureTransitionExists()
        {
            if (_transition is not null)
                return _transition;

            _transition = new LatexTransition(fromState, toState, IPrimerAnimation.cubic);
            from.transform.CopyTo(_transition.transform);
            return _transition;
        }

        private void ResetTransition(LatexTransition value)
        {
            if (_transition is not null)
                _transition.Dispose();

            _transition = value;
        }
        #endregion

        private enum State { Unset, Initial, Transitioning, Ended }
        private State state = State.Unset;

        private LatexTransitionState fromState => from.state;
        private LatexTransitionState toState => new(to, groups);


        public override void Cleanup()
        {
            base.Cleanup();
            SetInitialState();
        }

        public override void Prepare()
        {
            base.Prepare();
            EnsureTransitionExists();
        }

        [UsedImplicitly]
        public void Update(float time)
        {
            switch (time) {
                case <= 0:
                    SetInitialState();
                    break;
                case >= 1:
                    SetEndState();
                    break;
                default:
                    SetTransitionState(time);
                    break;
            }
        }


        private void SetInitialState()
        {
            if (state == State.Initial)
                return;

            state = State.Initial;
            transition = null;
            from.gameObject.SetActive(true);
            to.gameObject.SetActive(false);
        }

        private void SetEndState()
        {
            if (state == State.Ended)
                return;

            state = State.Ended;
            transition = null;
            from.gameObject.SetActive(false);
            to.gameObject.SetActive(true);

            from.transform.CopyTo(toState.transform, offsetPosition: fromState.GetOffsetWith(toState));
        }

        private void SetTransitionState(float time)
        {
            if (state != State.Transitioning) {
                state = State.Transitioning;
                from.gameObject.SetActive(false);
                to.gameObject.SetActive(false);
            }

            transition.Apply(time);
        }

        private void UpdateGroupsLength()
        {
            if (from == null || to == null)
                return;

            var length = Mathf.Max(from.ranges.Count, to.ranges.Count);
            groups.SetLength(length);
        }

        public void Dispose()
        {
            _transition?.Dispose();
        }
    }
}
