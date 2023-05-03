using UnityEngine;
using System.Collections.Generic;

namespace Primer.Animation
{
    public static class SpecializedTweens
    {
        public static Tween PulseGroup(List<Transform> objectsToPulse, float sizeFactor = 1.2f, float attack = 0.5f, float hold = 0.5f, float decay = 0.5f)
        {
            // Calculate the center of the group in world/global space
            Vector3 center = CalculateGroupCenter(objectsToPulse);

            // Create a list to store the Tweens
            List<Tween> tweens = new List<Tween>();

            // For each transform in objectsToPulse
            foreach (Transform objTransform in objectsToPulse)
            {
                var localScale = objTransform.localScale;
                var originalPosition = objTransform.position;
                var direction = (objTransform.position - center).normalized;
                var distance = Vector3.Distance(originalPosition, center) * (sizeFactor - 1);

                // Create the Tweens and add them to the list
                tweens.Add(Tween.Series(
                    Tween.Parallel(
                        objTransform.ScaleTo(localScale * sizeFactor) with { duration = attack },
                        objTransform.MoveTo(originalPosition + direction * distance, useGlobalPosition: true) with { duration = attack }
                    ),
                    Tween.noop with { duration = hold },
                    Tween.Parallel(
                        objTransform.ScaleTo(localScale, initialScale: localScale * sizeFactor) with { duration = decay },
                        objTransform.MoveTo(originalPosition, initialPosition: originalPosition + direction * distance, useGlobalPosition: true) with { duration = decay }
                    )
                ));
            }

            // Return a Parallel Tween containing all the Tweens created in the loop
            return Tween.Parallel(tweens.ToArray());
        }

        private static Vector3 CalculateGroupCenter(List<Transform> objectsToPulse)
        {
            Vector3 center = Vector3.zero;
            foreach (Transform objTransform in objectsToPulse)
            {
                center += objTransform.position;
            }
            return center / objectsToPulse.Count;
        }
    }
}