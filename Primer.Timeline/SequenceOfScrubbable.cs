using System.Collections.Generic;
using Primer.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Timeline
{
    public abstract class SequenceOfScrubbable<T> : Sequence
        where T : Scrubbable, new()
    {
        public float delay = 0f;
        public float duration = 0.5f;
        public EaseMode ease = EaseMode.Cubic;

        [Space]
        [HideLabel]
        [InlineProperty]
        public T scrubbable = new();

        public override void Cleanup()
        {
            scrubbable.Cleanup();
        }

        public override void Prepare()
        {
            scrubbable.Prepare();
        }

        public override async IAsyncEnumerator<Tween> Run()
        {
            yield return scrubbable.AsTween() with {
                delay = delay,
                duration = duration,
                easing = ease.GetMethod(),
            };
        }
    }
}
