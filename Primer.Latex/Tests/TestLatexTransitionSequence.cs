using System.Collections.Generic;
using Primer.Animation;
using Primer.Timeline;

namespace Primer.Latex
{
    public class TestLatexTransitionSequence : Sequence
    {
        public LatexComponent transitionTo;

        private LatexComponent target;
        private LatexScrubbable scrubbable;

        public override void Cleanup()
        {
            base.Cleanup();
            scrubbable?.Dispose();
            scrubbable = null;
        }

        public override void Prepare()
        {
            base.Prepare();
            target = GetComponent<LatexComponent>();

            scrubbable = target.CreateTransition(
                transitionTo,
                GroupTransitionType.Remove,
                GroupTransitionType.Anchor,
                GroupTransitionType.Replace
            );
        }

        public override async IAsyncEnumerator<Tween> Run()
        {
            yield return scrubbable.AsTween();
        }
    }
}
