using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Primer.Timeline;
using UnityEngine;

namespace Primer.Tools.Tests
{
    public class AnimateArrow : Triggerable
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
                arrow.start = startInitial.Value;

            if (endInitial is not null)
                arrow.end = endInitial.Value;

            arrow.Recalculate();
        }

        public override void Prepare()
        {
            arrow = GetComponent<PrimerArrow2>();
            startInitial = arrow.start;
            endInitial = arrow.end;
        }


        [UsedImplicitly]
        public void Animate()
        {
            arrow.Animate(start, end).Forget();
        }
    }
}
