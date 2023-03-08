using System.Collections.Generic;
using Primer.Animation;

namespace Primer.Timeline
{
    public abstract class Sequence : AsyncMonoBehaviour
    {
        public virtual void Prepare() {}

        public virtual void Cleanup() {}

        public abstract IAsyncEnumerator<Tween> Run();


        protected static Tween Parallel(params Tween[] tweenList)
        {
            return Tween.Merge(tweenList);
        }
    }
}
