using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Primer.Animation;
using Primer.Timeline;
using Sirenix.Utilities;
using UnityEngine;

namespace Primer.Latex
{
    public abstract class LatexSequence : Sequence
    {
        protected abstract IEnumerator<TweenProvider> LatexStages();

        protected LatexComponent Latex(string formula, string name = null)
        {
            var child = _gnome.AddLatex(formula);
            child.transform.SetDefaults();

            lastCreatedLatex = child;
            return child;
        }

        protected TweenProvider Transition(string formula)
            => Transition(Array.Empty<int>(), Latex(formula), Array.Empty<int>(), GroupTransitionType.Replace);

        protected TweenProvider Transition(LatexComponent stage, params GroupTransitionType[] transition)
            => Transition(Array.Empty<int>(), stage, Array.Empty<int>(), transition);

        protected Tween Transition(int[] groupIndexesBefore, LatexComponent stage, int[] groupIndexesAfter, params GroupTransitionType[] transitions)
        {
            stages.Add(new Stage(
                latex: stage,
                new TransitionList(transitions),
                groupIndexesBefore,
                groupIndexesAfter
            ));
            stage.gameObject.SetActive(false);

            var prev = activeLatex ? activeLatex : initial;
            
            return ((Tween)new TweenProvider(() => {
                var transition = prev.Transition(groupIndexesBefore, stage.expression, groupIndexesAfter, transitions);
                runningTransition = transition.tween;
                stage.transform.localPosition = prev.transform.localPosition + transition.destinationOffset;
                return transition.tween;
            })).Observe(onComplete: () =>
            {
                prev.gameObject.SetActive(false);
                stage.gameObject.SetActive(true);
            });
        }


        #region Internals
        [Serializable]
        public record Stage(
            LatexComponent latex,
            TransitionList transition,
            [CanBeNull] int[] groupIndexesBefore = null,
            [CanBeNull] int[] groupIndexesAfter = null
        );

        private Gnome _gnome;
        private LatexComponent lastCreatedLatex;
        public LatexComponent activeLatex { get; private set; }
        private Tween runningTransition;
        private bool isInitialized = false;

        internal LatexComponent initial;
        private readonly List<Stage> stages = new();
        private readonly List<Stage> allStages = new();

        public override void Cleanup()
        {
            base.Cleanup();
            CleanupAsync();
        }

        private async void CleanupAsync()
        {
            await EnsureIsInitialized();

            if (runningTransition is not null) {
                runningTransition.Evaluate(0);
                runningTransition.Dispose();
                runningTransition = null;
            }

            DisableObjects();
        }

        public override async IAsyncEnumerator<Tween> Define()
        {
            if (!PrimerTimeline.isPreloading)
                await EnsureIsInitialized();

            transform.GetChildren().ForEach(
                child =>
                {
                    child.DisableChildren();
                    child.gameObject.SetActive(false);
                } 
            );
            
            stages.Clear();
            initial = null;

            _gnome = new Gnome(transform);

            var enumerator = LatexStages();
            enumerator.MoveNext();

            initial = lastCreatedLatex;

            do {
                activeLatex = lastCreatedLatex;
                yield return enumerator.Current;
            } while (enumerator.MoveNext());

            _gnome.Purge();

            yield return activeLatex.ScaleTo(0);
        }

        private void DisableObjects()
        {
            if (this == null)
                return;

            foreach (var child in transform.GetChildren())
                child.SetActive(false);
        }

        internal async UniTask EnsureIsInitialized()
        {
            if (isInitialized)
                return;

            isInitialized = true;
            await RunTrough();
        }

        internal async UniTask RunTrough()
        {
            var enumerator = Define();

            while (await enumerator.MoveNextAsync()) {}

            allStages.Clear();
            allStages.AddRange(stages);

            DisableObjects();
            isInitialized = true;
        }

        internal List<Stage> GetStages()
        {
            if (allStages.Count is 0 || initial == null || stages.Any(x => x.latex == null)) {
                RunTrough().Forget();
            }

            return allStages;
        }
        #endregion
    }
}
