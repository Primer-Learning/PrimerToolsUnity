using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer.Animation;

namespace Primer.Latex
{
    public partial class LatexComponent
    {
        #region Transition(string)
        public UniTask<Tween> Transition(string to)
        {
            return Transition(Array.Empty<int>(), to, Array.Empty<int>(), GroupTransitionType.Replace);
        }

        public UniTask<Tween> Transition(string from, string to)
        {
            return Transition(from, Array.Empty<int>(), to, Array.Empty<int>(), GroupTransitionType.Replace);
        }

        public async UniTask<Tween> Transition(
            IEnumerable<int> fromGroups,
            string to, IEnumerable<int> toGroups,
            params GroupTransitionType[] transitions)
        {
            var toExpression = await ProcessWithoutQueue(to);
            return Transition(fromGroups, toExpression, toGroups, transitions);
        }

        public async UniTask<Tween> Transition(
            string from, IEnumerable<int> fromGroups,
            string to, IEnumerable<int> toGroups,
            params GroupTransitionType[] transitions)
        {
            var (fromExpression, toExpression) = await UniTask.WhenAll(
                ProcessWithoutQueue(from),
                ProcessWithoutQueue(to)
            );

            return Transition(fromExpression, fromGroups, toExpression, toGroups, transitions);
        }
        #endregion


        #region Transition(LatexComponent)
        public Tween Transition(LatexComponent to)
        {
            return Transition(
                expression,
                Array.Empty<int>(),
                to.expression,
                Array.Empty<int>(),
                GroupTransitionType.Replace
            );
        }

        public Tween Transition(
            IEnumerable<int> fromGroups,
            LatexComponent to, IEnumerable<int> toGroups,
            params GroupTransitionType[] transitions)
        {
            return Transition(expression, fromGroups, to.expression, toGroups, transitions);
        }
        #endregion


        #region Transition(LatexExpression)
        public Tween Transition(LatexExpression to)
        {
            return Transition(
                expression,
                Array.Empty<int>(),
                to,
                Array.Empty<int>(),
                GroupTransitionType.Replace
            );
        }


        public Tween Transition(
            IEnumerable<int> fromGroups,
            LatexExpression to, IEnumerable<int> toGroups,
            params GroupTransitionType[] transitions)
        {
            return Transition(expression, fromGroups, to, toGroups, transitions);
        }
        #endregion


        // Actual implementation
        public Tween Transition(
            LatexExpression from, IEnumerable<int> fromGroups,
            LatexExpression to, IEnumerable<int> toGroups,
            params GroupTransitionType[] transitions)
        {
            RemovePreviousTransitions();

            var start = gameObject.AddComponent<GroupedLatex>().Set(
                "Transition start",
                fromGroups.ToArray(),
                from
            );

            var end = gameObject.AddComponent<GroupedLatex>().Set(
                "Transition end",
                toGroups.ToArray(),
                to
            );

            var transition = gameObject.AddComponent<LatexTransition>();
            transition.Set(start, end, new TransitionList(transitions));
            transition.Deactivate();
            return transition.ToTween();
        }


        #region Helpers
        private async UniTask<LatexExpression> ProcessWithoutQueue(string code)
        {
            // We use a different processor to not be involved in this component's processing queue
            var separateProcessor = LatexProcessor.GetInstance();
            var newConfig = new LatexInput(code, _headers);
            return await separateProcessor.Process(newConfig);
        }

        private void RemovePreviousTransitions()
        {
            // Remove all children except for the characters gnome
            var container = new Gnome(transform);
            container.Insert(characters);
            container.Purge();

            GetComponent<LatexTransition>()?.DisposeComponent();

            foreach (var group in GetComponents<GroupedLatex>()) {
                group.DisposeComponent();
            }
        }
        #endregion
    }
}
