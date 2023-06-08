using UnityEngine;

namespace Primer.Animation
{
    public static class IPrimer_MoveAndRotateExtensions
    {
        public static Tween MoveAndRotate(this IPrimer self, Vector3 newPosition, Quaternion newRotation)
        {
            return Tween.Parallel(
                self.MoveTo(newPosition, newPosition),
                self.RotateTo(newRotation)
            );
        }

        public static Tween MoveAndRotate(this Component self, Vector3 newPosition, Quaternion newRotation)
        {
            return Tween.Parallel(
                self.MoveTo(newPosition, newPosition),
                self.RotateTo(newRotation)
            );
        }
    }
}
