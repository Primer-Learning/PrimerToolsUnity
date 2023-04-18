using System.Collections.Generic;
using Primer.Animation;
using Primer.Timeline;
using UnityEngine;

namespace Primer.Shapes.Tests
{
    public class AnimateArrow : Sequence
    {
        public Vector3 start = Vector3.one;
        public Vector3 end = Vector3.one * 3;

        private PrimerArrow2 arrow;

        public Vector3? startInitial;
        public Vector3? endInitial;


        public override void Cleanup()
        {
            arrow = GetComponent<PrimerArrow2>();

            if (startInitial is not null)
                arrow.tailPoint.vector = startInitial.Value;

            if (endInitial is not null)
                arrow.headPoint.vector = endInitial.Value;

            arrow.Recalculate();
        }

        public override void Prepare()
        {
            arrow = GetComponent<PrimerArrow2>();
            startInitial = arrow.tail;
            endInitial = arrow.head;
        }

        public override async IAsyncEnumerator<Tween> Run()
        {
            yield return arrow.Animate(start, end);
        }
    }
}
