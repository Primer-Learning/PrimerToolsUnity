using System.Collections.Generic;
using Primer.Animation;
using Primer.Shapes;
using Primer.Timeline;
using UnityEngine;

namespace Test_scenes.Arrow
{
    public class ArrowTestSequence : Sequence
    {
        public PrimerArrow3 testArrow;
        
        public override async IAsyncEnumerator<Tween> Define()
        {
            testArrow.length = 1;
            
            yield return Tween.Value(
                value => testArrow.length = value,
                () => testArrow.length,
                () => 4
            );
        }
    }
}