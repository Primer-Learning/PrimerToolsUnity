using System.Collections.Generic;

namespace Primer.Timeline
{
    public class PlayableTimeComparer : IComparer<GenericBehaviour>
    {
        public int Compare(GenericBehaviour left, GenericBehaviour right)
        {
            if (left is null && right is null)
                return 0;

            if (left is null)
                return 1;

            if (right is null)
                return -1;

            var delta = left.start - right.start;
            return (int) (delta * 10000);
        }
    }
}
