using UnityEngine;

namespace Primer.Timeline
{
    public static class IPoolExtensions
    {
        public static IPool<T> ForTimeline<T>(this IPool<T> self) where T : Component
        {
            return self.Specialize(PrimerTimeline.MarkAsEphemeral);
        }
    }
}
