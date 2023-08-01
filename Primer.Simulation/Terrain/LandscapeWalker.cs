using Primer.Animation;
using Primer.Shapes;
using UnityEngine;

namespace Primer.Simulation
{
    public class LandscapeWalker : LandscapeItem
    {
        private const float DEFAULT_SPEED = 20;
        private const float DEFAULT_TURN_SPEED = DEFAULT_SPEED * 4;

        public Tween WalkTo(Vector2 to, float stopDistance = 0, float forcedDuration = -1)
        {
            return WalkToImpl(landscape.GetGroundAt(to), stopDistance, forcedDuration);
        }

        public Tween WalkToLocal(Vector2 to, float stopDistance = 0, float forcedDuration = -1)
        {
            return WalkToImpl(landscape.GetGroundAtLocal(to), stopDistance, forcedDuration);
        }

        public Tween WalkTo(Component to, float stopDistance = 0, float forcedDuration = -1)
        {
            return WalkToImpl(new Vector3Provider(() => landscape.GetGroundAt(to.transform.position)), stopDistance, forcedDuration);
        }

        private Tween WalkToImpl(Vector3Provider to, float stopDistance, float forcedDuration)
        {
            var ignoreHeight = new Vector3(1, 0, 1);
            var myTransform = transform;
            var initialPosition = myTransform.position;
            var initialRotation = myTransform.rotation;

            return new Tween(
                t => {
                    var targetPosition = to.value;

                    // we want to walk _to_ the target, no walk over it
                    var directionVector = targetPosition - initialPosition;
                    var destination = targetPosition - directionVector.normalized * stopDistance;

                    myTransform.position = landscape.GetGroundAt(Vector3.Lerp(initialPosition, destination, t));

                    var lookRotation = (targetPosition - myTransform.position).ElementWiseMultiply(ignoreHeight);
                    if (lookRotation == Vector3.zero)
                        return;

                    var targetRotation = Quaternion.LookRotation(lookRotation);
                    myTransform.rotation = Quaternion.Lerp(initialRotation, targetRotation, t * DEFAULT_TURN_SPEED);
                }
            ) {
                duration = forcedDuration < 0 ? Vector3.Distance(initialPosition, to.value) / DEFAULT_SPEED : forcedDuration
            };
        }
    }
}
