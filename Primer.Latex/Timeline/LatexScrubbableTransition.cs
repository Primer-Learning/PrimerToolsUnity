using System.Collections.Generic;
using JetBrains.Annotations;

namespace Primer.Latex
{
    [UsedImplicitly]
    public class LatexScrubbableTransition : Scrubbable
    {
        public LatexRenderer from;
        public LatexRenderer to;
        public List<TransitionType> transitions = new();

        private LatexTransition transition;

        public override void Cleanup()
        {
            base.Cleanup();
            transition.Dispose();
        }

        public override void Prepare()
        {
            base.Prepare();
            transition = from.CreateTransition(to,transitions);
        }

        public void Update(float t)
        {
            transition.Apply(t);
        }
    }
}
