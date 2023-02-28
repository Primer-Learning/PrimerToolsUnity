using System.Threading.Tasks;
using Primer.Animation;
using Primer.Timeline;

namespace Primer.Latex
{
    public class TestLatexTransitionTrigger : Triggerable
    {
        public LatexComponent transitionTo;

        private LatexComponent target;

        public override void Prepare()
        {
            base.Prepare();
            target = GetComponent<LatexComponent>();
        }

        public async Task Transition()
        {
            await target.TransitionTo(
                transitionTo,
                new [] {
                    TransitionType.Transition,
                    TransitionType.Replace,
                    TransitionType.Anchor,
                    TransitionType.Transition,
                    TransitionType.Replace,
                    TransitionType.Transition,
                    TransitionType.Replace,
                    TransitionType.Transition,
                    TransitionType.Transition,
                    TransitionType.Transition,
                    TransitionType.Replace,
                },
                new Tweener(2f)
            );
        }
    }
}
