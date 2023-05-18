using UnityEngine;

namespace Primer.Animation
{
    public static class PrimerAnimationContainerExtensions
    {
        public static Tween ScaleUpFromZero<T>(this Container<T> self) where T : Component
        {
            return self.primer.ScaleUpFromZero();
        }

        public static Tween ScaleTo<T>(this Container<T> self, float newScale, float? initialScale = null) where T : Component
        {
            return self.transform.ScaleTo(newScale, initialScale);
        }

        public static Tween ScaleTo<T>(this Container<T> self, Vector3 newScale, Vector3? initialScale = null) where T : Component
        {
            return self.transform.ScaleTo(newScale, initialScale);
        }

        public static Tween MoveTo<T>(this Container<T> self, Vector3 newPosition, Vector3? initialPosition = null, bool useGlobalPosition = false)where T : Component
        {
            return self.transform.MoveTo(newPosition, initialPosition, useGlobalPosition);
        }

        public static Tween MoveBy<T>(this Container<T> self, Vector3 displacement) where T : Component
        {
            return self.transform.MoveBy(displacement);
        }

        public static Tween RotateTo<T>(this Container<T> self, Quaternion newRotation, Quaternion? initialRotation = null) where T : Component
        {
            return self.transform.RotateTo(newRotation, initialRotation);
        }

        public static Tween Pulse<T>(this Container<T> self, float sizeFactor = 1.2f, float attack = 0.5f, float hold = 0.5f, float decay = 0.5f)where T : Component
        {
            return self.transform.Pulse(sizeFactor, attack, hold, decay);
        }

        public static Tween PulseAndWobble<T>(
            this Container<T> self,
            float sizeFactor = 1.2f,
            float attack = 0.5f,
            float hold = 0.5f,
            float decay = 0.5f,
            float wobbleAngle = 5,
            float wobbleCyclePeriod = 0.75f)
            where T : Component
        {
            return self.transform.PulseAndWobble(sizeFactor, attack, hold, decay, wobbleAngle, wobbleCyclePeriod);
        }
    }
}
