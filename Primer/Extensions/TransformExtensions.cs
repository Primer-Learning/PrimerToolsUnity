using UnityEngine;

namespace Primer
{
    public static class TransformExtensions
    {
        public static TransformSnapshot Snapshot(this Transform transform) => new TransformSnapshot(transform);
    }
}
