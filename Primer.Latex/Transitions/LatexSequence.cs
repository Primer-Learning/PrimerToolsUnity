using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Primer.Animation;
using Primer.Timeline;

namespace Primer.Latex
{
    public abstract class LatexSequence : Sequence
    {
        protected abstract IEnumerator<TweenProvider> LatexStages();

        protected LatexComponent Latex(string formula, string name = null)
        {
            var child = container.AddLatex(formula);
            child.transform.SetDefaults();
            child.SetActive(false);

            lastCreatedLatex = child;
            return child;
        }

        protected TweenProvider Transition(string formula)
            => Transition(null, Latex(formula), null, GroupTransitionType.Replace);

        protected TweenProvider Transition(LatexComponent stage, params GroupTransitionType[] transition)
            => Transition(null, stage, null, transition);

        // protected TweenProvider Transition(int[] groupIndexesBefore, LatexComponent stage, params GroupTransitionType[] transition)
        //     => Transition(groupIndexesBefore, stage, null, transition);

        // protected TweenProvider Transition(LatexComponent stage, int[] groupIndexesAfter, params GroupTransitionType[] transition)
        //     => Transition(null, stage, groupIndexesAfter, transition);

        protected TweenProvider Transition(int[] groupIndexesBefore, LatexComponent stage, int[] groupIndexesAfter, params GroupTransitionType[] transition)
        {
            stages.Add(new Stage(
                latex: stage,
                transition,
                groupIndexesBefore,
                groupIndexesAfter
            ));

            var prev = currentStage ?? initial;

            return new TweenProvider(() => {
                if (groupIndexesBefore is not null)
                    prev.SetGroupIndexes(groupIndexesBefore);

                if (groupIndexesAfter is not null)
                    stage.SetGroupIndexes(groupIndexesAfter);

                runningScrubbable = prev.CreateTransition(stage, transition);
                return runningScrubbable.AsTween();
            });
        }


        #region Internals
        [Serializable]
        public record Stage(
            LatexComponent latex,
            GroupTransitionType[] transition,
            [CanBeNull] int[] groupIndexesBefore = null,
            [CanBeNull] int[] groupIndexesAfter = null
        );

        private Container container;
        private LatexComponent lastCreatedLatex;
        public LatexComponent currentStage { get; private set; }
        private LatexScrubbable runningScrubbable;
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

            if (runningScrubbable is not null) {
                runningScrubbable.Dispose();
                runningScrubbable = null;
            }

            DisableObjects();
        }

        public override async IAsyncEnumerator<Tween> Run()
        {
            await EnsureIsInitialized();

            stages.Clear();
            initial = null;

            container = new Container(transform);

            var enumerator = LatexStages();
            enumerator.MoveNext();

            initial = lastCreatedLatex;

            do {
                currentStage = lastCreatedLatex;
                yield return enumerator.Current;
            } while (enumerator.MoveNext());

            container.Purge();

            yield return currentStage.ScaleTo(0);
        }

        private void DisableObjects()
        {
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
            var enumerator = Run();

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
