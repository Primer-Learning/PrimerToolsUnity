using System.Collections.Generic;

namespace Primer.Timeline
{
    public class PlayableTimeComparer : IComparer<PrimerPlayable>
    {
        public int Compare(PrimerPlayable left, PrimerPlayable right)
        {
            return (left, right) switch {
                (null, null) => 0,
                (null, _) => 1,
                (_, null) => -1,
                _ => (int) ((left.start - right.start) * 10000),
            };
        }
    }
}
