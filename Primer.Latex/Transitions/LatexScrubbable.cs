using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Primer.Timeline;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Primer.Latex
{
    [Serializable]
    public class LatexScrubbable : Scrubbable, IDisposable
    {
        #region Inspector
        [SerializeReference, HideInInspector]
        private LatexComponent _from;

        [ShowInInspector]
        public LatexComponent from {
            get => _from;
            set {
                _from = value == null ? null : value;
                transition = null;
                UpdateGroupsLength();
            }
        }

        [SerializeReference, HideInInspector]
        private LatexComponent _to;

        [ShowInInspector]
        public LatexComponent to {
            get => _to;
            set {
                _to = value == null ? null : value;
                transition = null;
                UpdateGroupsLength();
            }
        }

        [SerializeReference, HideInInspector]
        private List<TransitionType> _groups = new();

        [ShowInInspector]
        [LatexTransitionGroup(nameof(_from), nameof(_to))]
        public List<TransitionType> groups {
            get => _groups;
            set {
                _groups = value?.ToList() ?? new List<TransitionType>();
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

            _transition = new LatexTransition(from, to, groups);

            state = State.Transitioning;
            from.gameObject.SetActive(false);
            to.gameObject.SetActive(false);
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
        private bool isValid => from != null && to != null;

        public override void Cleanup()
        {
            if (from == null)
                return;

            base.Cleanup();
            SetInitialState();
        }

        public override void Prepare()
        {
            if (!isValid)
                return;

            base.Prepare();
            EnsureTransitionExists();
        }

        [UsedImplicitly]
        public override void Update(float time)
        {
            if (!isValid)
                return;

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

            if (to != null)
                to.gameObject.SetActive(false);
        }

        private void SetEndState()
        {
            if (state == State.Ended)
                return;

            var offset = transition.GetOffset();

            state = State.Ended;
            transition = null;
            from.gameObject.SetActive(false);
            to.gameObject.SetActive(true);

            from.transform.CopyTo(to.transform, offsetPosition: offset);
        }

        private void SetTransitionState(float time)
        {
            transition.Apply(time);
        }

        private void UpdateGroupsLength()
        {
            if (from == null || to == null)
                return;

            var length = Mathf.Max(from.groupsCount, to.groupsCount);
            _groups.SetLength(length);
        }

        public void Dispose()
        {
            Cleanup();
            _transition?.Dispose();
        }
    }
}
