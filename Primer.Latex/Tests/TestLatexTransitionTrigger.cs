using Cysharp.Threading.Tasks;
using Primer.Animation;
using Primer.Timeline;

namespace Primer.Latex
{
    public class TestLatexTransitionTrigger : Triggerable
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
                TransitionType.Remove,
                TransitionType.Anchor,
                TransitionType.Replace
            );
        }

        public async UniTask Transition()
        {
            await scrubbable.Play();
        }
    }
}
