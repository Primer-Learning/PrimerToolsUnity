using System.Collections.Generic;
using System.Linq;
using Primer.Animation;
using Primer.Timeline;
using UnityEngine;

namespace Primer.Latex
{
    public abstract class LatexSequence : Sequence
    {
        protected abstract IEnumerator<Tween> LatexStages();

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

        protected Tween Transition(LatexComponent stage, params TransitionType[] transition)
        {
            stages.Add(stage);
            transitions.Add(transition);

            var prev = currentLatex ?? initial;
            runningScrubbable = prev.CreateTransition(stage, transition);
            return runningScrubbable.AsTween();
        }

        protected Tween Transition(string formula)
        {
            return Transition(Latex(formula), TransitionType.Replace);
        }

        #region Internals
        private ChildrenDeclaration children;
        private LatexComponent lastCreatedLatex;
        private LatexComponent currentLatex;
        private LatexScrubbable runningScrubbable;

        internal LatexComponent initial;
        private readonly List<LatexComponent> stages = new();
        private readonly List<TransitionType[]> transitions = new();
        private readonly List<LatexComponent> allStages = new();
        private readonly List<TransitionType[]> allTransitions = new();

        public override void Cleanup()
        {
            base.Cleanup();

            if (initial == null)
                RunTrough();

            if (runningScrubbable is not null) {
                runningScrubbable.Dispose();
                runningScrubbable = null;
            }

            DisableObjects();
        }

        public override async IAsyncEnumerator<Tween> Run()
        {
            stages.Clear();
            transitions.Clear();
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
        }

        private void DisableObjects()
        {
            initial.SetActive(false);

            foreach (var stage in stages)
                stage.SetActive(false);
        }

        internal async void RunTrough()
        {
            var enumerator = Run();
            while (await enumerator.MoveNextAsync()) {}

            allStages.Clear();
            allStages.AddRange(stages);

            allTransitions.Clear();
            allTransitions.AddRange(transitions);

            DisableObjects();
        }
        #endregion


        #region Editor
        internal List<(LatexComponent latex, TransitionType[] transition)> GetStages()
        {
            return allStages.Zip(allTransitions, (latex, transition) => (latex, transition)).ToList();
        }
        #endregion
    }
}
