using System.Globalization;
using System;
using Primer.Animation;
using Primer.Shapes;
using UnityEngine;

namespace Primer.Simulation
{
    public class LandscapeWalker : LandscapeItem
    {
        private const float DEFAULT_SPEED = 20;
        private const float DEFAULT_TURN_SPEED = DEFAULT_SPEED * 4;

        public Tween WalkTo(Transform to, float stopDistance = 0, float forcedDuration = -1, Vector3 offset = default)
        {
            offset = transform.parent.TransformVector(offset);
            return WalkToImpl(landscape.GetGroundAtWorldPoint(to.position + offset), stopDistance, forcedDuration);
        }

        public Tween WalkToLocal(Vector2 to, float stopDistance = 0, float forcedDuration = -1)
        {
            return WalkToImpl(landscape.GetGroundAtLocalPoint(to), stopDistance, forcedDuration);
        }

        public Tween WalkTo(Component to, float stopDistance = 0, float forcedDuration = -1, Vector3 offset = default)
        {
            return WalkTo(to.transform, stopDistance, forcedDuration, offset);
        }

        private Tween WalkToImpl(Vector3 to, float stopDistance, float forcedDuration)
        {
            var ignoreHeight = new Vector3(1, 0, 1);
            var myTransform = transform;

            
            var targetPosition = to;

            // We want to walk _to_ the target, no walk over it
            var directionVector = targetPosition - myTransform.position;
            var destination = targetPosition - directionVector.normalized * (stopDistance * landscape.transform.lossyScale.x);

            float DurationCalculation() =>
                forcedDuration > 0
                    ? forcedDuration
                    : Vector3.Distance(myTransform.position, to) / DEFAULT_SPEED;

            var moveTween = Tween.Value(
                v =>
                {
                    myTransform.position = landscape.GetGroundAtWorldPoint(v);
                },
                () => myTransform.position,
                () => destination,
                (Func<float>)DurationCalculation
            );
            

            // return moveTween;

            var rotateTween = Tween.noop;
            var lookRotation = (targetPosition - myTransform.position).ElementWiseMultiply(ignoreHeight);
            if (lookRotation != Vector3.zero)
            {
                var targetRotation = Quaternion.LookRotation(lookRotation);
            
                rotateTween = Tween.Value(
                    r => myTransform.localRotation = r,
                    () => myTransform.localRotation,
                    () => targetRotation,
                    () => Mathf.Min(DurationCalculation(), 0.5f)
                );
            }


            return Tween.Parallel(moveTween, rotateTween);
        }
    }
}
