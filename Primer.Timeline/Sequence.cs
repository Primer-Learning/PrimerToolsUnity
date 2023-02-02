using System.Collections.Generic;

namespace Primer.Timeline
{
    public abstract class Sequence : AsyncMonoBehaviour
    {
        public virtual void Prepare() {}

        public virtual void Cleanup() {}

        public abstract IAsyncEnumerator<object> Run();
    }
}
