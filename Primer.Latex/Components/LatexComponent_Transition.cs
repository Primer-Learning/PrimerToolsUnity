using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer.Animation;

namespace Primer.Latex
{
    public partial class LatexComponent
    {
        public UniTask<Tween> Transition(string newLatex)
        {
            return Transition(Array.Empty<int>(), newLatex, Array.Empty<int>(), GroupTransitionType.Replace);
        }

        public UniTask<Tween> Transition(string startLatex, string endLatex)
        {
            return Transition(
                startLatex,
                Array.Empty<int>(),
                endLatex,
                Array.Empty<int>(),
                GroupTransitionType.Replace
            );
        }

        public async UniTask<Tween> Transition(
            IEnumerable<int> startGroups,
            string endLatex, IEnumerable<int> endGroups,
            params GroupTransitionType[] transitions)
        {
            var toExpression = await ProcessWithoutQueue(endLatex);
            return Transition(startGroups, toExpression, endGroups, transitions);
        }

        public async UniTask<Tween> Transition(
            string startLatex, IEnumerable<int> startGroups,
            string endLatex, IEnumerable<int> endGroups,
            params GroupTransitionType[] transitions)
        {
            var (fromExpression, toExpression) = await UniTask.WhenAll(
                ProcessWithoutQueue(startLatex),
                ProcessWithoutQueue(endLatex)
            );

            return Transition(fromExpression, startGroups, toExpression, endGroups, transitions);
        }

        public Tween Transition(
            IEnumerable<int> startGroups,
            LatexExpression endExpression, IEnumerable<int> endGroups,
            params GroupTransitionType[] transitions)
        {
            return Transition(expression, startGroups, endExpression, endGroups, transitions);
        }

        public Tween Transition(
            LatexExpression startExpression, IEnumerable<int> startGroups,
            LatexExpression endExpression, IEnumerable<int> endGroups,
            params GroupTransitionType[] transitions)
        {
            RemovePreviousTransitions();

            var start = gameObject.AddComponent<GroupedLatex>().Set(
                "Transition start",
                startGroups.ToArray(),
                startExpression
            );

            var end = gameObject.AddComponent<GroupedLatex>().Set(
                "Transition end",
                endGroups.ToArray(),
                endExpression
            );

            var transition = gameObject.AddComponent<LatexTransition>();
            transition.Set(start, end, new TransitionList(transitions));
            transition.Deactivate();
            return transition.ToTween();
        }

        private async UniTask<LatexExpression> ProcessWithoutQueue(string code)
        {
            // We use a different processor to not be involved in this component's processing queue
            var separateProcessor = LatexProcessor.GetInstance();
            var newConfig = new LatexInput(code, _headers);
            return await separateProcessor.Process(newConfig);
        }

        private void RemovePreviousTransitions()
        {
            // Remove all children except for the characters container
            var container = new Container(transform);
            container.Insert(charactersContainer);
            container.Purge();

            GetComponent<LatexTransition>()?.DisposeComponent(urgent: true);

            foreach (var group in GetComponents<GroupedLatex>()) {
                group.DisposeComponent();
            }
        }
    }
}
