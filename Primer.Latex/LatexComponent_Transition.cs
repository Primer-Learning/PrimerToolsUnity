using System;
using Cysharp.Threading.Tasks;
using Primer.Animation;
using UnityEngine;

namespace Primer.Latex
{
    public partial class LatexComponent
    {
        public UniTask<Tween> Transition(string newLatex)
        {
            return Transition(Array.Empty<int>(), newLatex, Array.Empty<int>(), GroupTransitionType.Replace);
        }

        public async UniTask<Tween> Transition(int[] startGroups, string newLatex, int[] endGroups, params GroupTransitionType[] transitions)
        {
            // We use a different processor to not be involved in this component's processing queue
            var separateProcessor = LatexProcessor.GetInstance();
            var newConfig = new LatexInput(newLatex, _headers);
            var toExpression = await separateProcessor.Process(newConfig);
            return Transition(startGroups, toExpression, endGroups, transitions);
        }

        public Tween Transition(int[] fromGroups, LatexExpression toExpression, int[] toGroups, params GroupTransitionType[] transitions)
        {
            var fromExpression = expression;
            Transform root = null;
            LatexTransition transition = null;

            #region Transition creation / destruction is ugly
            void EnsureTransitionExists()
            {
                if (transition is not null)
                    return;

                var container = new Container(transform).AddContainer("Transition Root");
                root = container.transform;

                var fromContainer = container.Add("From");
                var from = new GroupedExpression(fromExpression, fromGroups);
                from.CreateGroupTransforms(fromContainer);
                fromContainer.SetActive(false);

                var toContainer = container.Add("To");
                var to = new GroupedExpression(toExpression, toGroups);
                to.CreateGroupTransforms(toContainer);
                toContainer.SetActive(false);

                var transitionContainer = container.Add("Transition");
                transition = new LatexTransition(from, to, transitions);
                transition.CreateTransforms(transitionContainer);

                charactersParent.SetActive(false);
                root.SetActive(true);
            }

            void DestroyTransition()
            {
                if (transition is null)
                    return;

                root.Dispose();
                root = null;
                transition = null;
            }
            #endregion

            void InitialState()
            {
                DestroyTransition();
                expression = fromExpression;
                UpdateChildren();
                root.SetActive(false);
                charactersParent.SetActive(true);
            }

            void FinalState()
            {
                DestroyTransition();
                expression = toExpression;
                UpdateChildren();
                root.SetActive(false);
                charactersParent.SetActive(true);
            }

            void ApplyTransition(float t)
            {
                EnsureTransitionExists();
                transition.Apply(t);
            }

            return new Tween(ApplyTransition).Observe(
                onInitialState: InitialState,
                onComplete: FinalState
            );
        }
    }
}
