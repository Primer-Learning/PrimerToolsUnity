using System;
using System.Threading.Tasks;
using Primer.Animation;
using Primer.Timeline;

namespace Primer.Latex
{
    public class TestLatexTransitionTrigger : Triggerable
    {
        public LatexRenderer transitionTo;

        private LatexRenderer target;
        private IDisposable transition;

        public override void Cleanup()
        {
            base.Cleanup();
            transition?.Dispose();
        }

        public override void Prepare()
        {
            base.Prepare();
            target = GetComponent<LatexRenderer>();
        }

        public async Task Transition()
        {
            transition = await target.TransitionTo(
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
