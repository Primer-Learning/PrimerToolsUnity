using System.Globalization;
using System;
using Primer.Animation;
using Primer.Shapes;
using UnityEngine;

namespace Primer.Simulation
{
    public class LandscapeWalker : LandscapeItem
    {
        private const float DEFAULT_SPEED = 15;
        private const float DEFAULT_TURN_SPEED = DEFAULT_SPEED * 4;

        public Tween WalkTo(Transform to, float stopDistance = 0, float forcedDuration = -1, Vector3 offset = default)
        {
            if (offset != default) offset = transform.parent.TransformVector(offset);
            return WalkToImpl(landscape.GetGroundAtWorldPoint(to.position), forcedDuration: forcedDuration, stopDistance: stopDistance, offset: offset);
        }
        public Tween WalkTo(Vector3 to, float stopDistance = 0, float forcedDuration = -1, Vector3 offset = default)
        {
            return WalkToImpl(landscape.GetGroundAtWorldPoint(to), stopDistance: stopDistance, forcedDuration: forcedDuration, offset: offset);
        }

        public Tween WalkToLocal(Vector2 to, float stopDistance = 0, float forcedDuration = -1)
        {
            return WalkToImpl(landscape.GetGroundAtLocalPoint(to), stopDistance: stopDistance, forcedDuration: forcedDuration);
        }

        public Tween WalkTo(Component to, float stopDistance = 0, float forcedDuration = -1, Vector3 offset = default)
        {
            return WalkTo(to.transform, stopDistance: stopDistance, forcedDuration: forcedDuration, offset);
        }

        private Tween WalkToImpl(Vector3 to, float forcedDuration, float stopDistance = 0, Vector3 offset = default, float finalTurnBeginFraction = 0.8f)
        {
            var ignoreHeight = new Vector3(1, 0, 1);
            var myTransform = transform;
            
            var targetPosition = to + offset; // Usually won't make sense to use both stopDistance and offset

            targetPosition += StopDistanceOffset();
            var finalDifference = (offset + StopDistanceOffset()).ElementWiseMultiply(ignoreHeight);
            
            var moveTween = Tween.Value(
                v =>
                {
                    myTransform.position = landscape.GetGroundAtWorldPoint(v);
                },
                () => myTransform.position,
                () => targetPosition,
                (Func<float>)DurationCalculation
            );
            var rotatePrepTween = Tween.Value(
                r => myTransform.localRotation = r,
                () => myTransform.localRotation,
                TargetPrepRotation,
                () => Mathf.Min(DurationCalculation() / 2, 0.5f)
            );
            var rotateFinishTween = Tween.Value(
                r => myTransform.localRotation = r,
                () => myTransform.localRotation,
                () => finalDifference == Vector3.zero ? myTransform.localRotation : Quaternion.LookRotation(-finalDifference),
                () => Mathf.Max(DurationCalculation(), 0.5f)
            );

            return Tween.Parallel(
                delayBetweenStarts: DurationCalculation() * finalTurnBeginFraction,
                Tween.Parallel(
                    rotatePrepTween,
                    moveTween
                ),
                rotateFinishTween
            );

            float DurationCalculation() =>
                forcedDuration > 0
                    ? forcedDuration
                    : Mathf.Max(Vector3.Distance(myTransform.position, targetPosition + StopDistanceOffset()) / DEFAULT_SPEED, 0.3f);
            
            Vector3 StopDistanceOffset() => (myTransform.position - targetPosition).normalized * (stopDistance * landscape.transform.lossyScale.x);
            Quaternion TargetPrepRotation() => (targetPosition - myTransform.position).ElementWiseMultiply(ignoreHeight) == Vector3.zero ? myTransform.localRotation : Quaternion.LookRotation((targetPosition - myTransform.position).ElementWiseMultiply(ignoreHeight));
        }
    }
}
