using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Primer.Animation;
using Primer.Timeline;
using UnityEngine;

namespace Primer.Latex
{
    public abstract class LatexSequence : Sequence
    {
        protected abstract IEnumerator<TweenProvider> LatexStages();

        protected LatexComponent Latex(string formula, string name = null)
        {
            var prefab = Resources.Load<LatexComponent>(LatexComponent.PREFAB_NAME);
            var childName = name ?? (initial ? $"Stage {stages.Count}" : "Initial");

            var child = children.NextIsInstanceOf(prefab, childName);
            child.transform.SetDefaults();
            child.Process(formula);
            child.SetActive(false);

            lastCreatedLatex = child;
            return child;
        }

        protected TweenProvider Transition(string formula)
            => Transition(Latex(formula), GroupTransitionType.Replace);

        protected TweenProvider Transition(LatexComponent stage, params GroupTransitionType[] transition)
            => Transition(null, stage, null, transition);

        protected TweenProvider Transition(int[] groupIndexesBefore, LatexComponent stage, params GroupTransitionType[] transition)
            => Transition(groupIndexesBefore, stage, null, transition);

        protected TweenProvider Transition(LatexComponent stage, int[] groupIndexesAfter, params GroupTransitionType[] transition)
            => Transition(null, stage, groupIndexesAfter, transition);

        protected TweenProvider Transition(int[] groupIndexesBefore, LatexComponent stage, int[] groupIndexesAfter, params GroupTransitionType[] transition)
        {
            stages.Add(new Stage(
                latex: stage,
                transition,
                groupIndexesBefore,
                groupIndexesAfter
            ));

            var prev = currentLatex ?? initial;

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

        private ChildrenDeclaration children;
        private LatexComponent lastCreatedLatex;
        private LatexComponent currentLatex;
        private LatexScrubbable runningScrubbable;
        private bool isInitialized = false;

        internal LatexComponent initial;
        private readonly List<Stage> stages = new();
        private readonly List<Stage> allStages = new();

        public override void Cleanup()
        {
            base.Cleanup();
            EnsureIsInitialized();

            if (runningScrubbable is not null) {
                runningScrubbable.Dispose();
                runningScrubbable = null;
            }

            DisableObjects();
        }

        public override async IAsyncEnumerator<Tween> Run()
        {
            EnsureIsInitialized();

            stages.Clear();
            initial = null;

            children = new ChildrenDeclaration(transform);

            var enumerator = LatexStages();
            enumerator.MoveNext();

            initial = lastCreatedLatex;

            do {
                currentLatex = lastCreatedLatex;
                yield return enumerator.Current;
            } while (enumerator.MoveNext());

            children.Apply();

            yield return currentLatex.ScaleTo(0);
        }

        private void DisableObjects()
        {
            initial.SetActive(false);

            foreach (var stage in stages)
                stage.latex.SetActive(false);
        }

        internal void EnsureIsInitialized()
        {
            if (isInitialized)
                return;

            isInitialized = true;
            RunTrough();
        }

        internal async void RunTrough()
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
            if (allStages.Count is not 0)
                return allStages;

            RunTrough();
            return allStages;
        }
        #endregion
    }
}
