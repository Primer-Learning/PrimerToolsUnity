using System.Collections.Generic;
using Primer.Animation;

namespace Primer.Timeline
{
    public static class AsyncEnumeratorExtensions
    {
        public static Tween NextClip(this IAsyncEnumerator<Tween> self)
        {
            self.MoveNextAsync();
            return self.Current;
        }
    }
}
