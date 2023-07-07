using Primer.Animation;
using Primer.Shapes;
using UnityEngine;

namespace Primer.Simulation
{
    public class LandscapeWalker : MonoBehaviour
    {
        private const float DEFAULT_SPEED = 8;
        private const float DEFAULT_TURN_SPEED = DEFAULT_SPEED * 4;

        private ISimulation simulationCache;
        private ISimulation simulation => transform.ParentComponent(ref simulationCache);

        public Tween WalkTo(Vector2 to)
        {
            return WalkToImpl(simulation.GetGroundAt(to));
        }

        public Tween WalkToLocal(Vector2 to)
        {
            return WalkToImpl(simulation.GetGroundAtLocal(to));
        }

        public Tween WalkTo(Component to)
        {
            return WalkToImpl(new Vector3Provider(() => simulation.GetGroundAt(to.transform.position)));
        }

        private Tween WalkToImpl(Vector3Provider to)
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
                    var destination = targetPosition - directionVector.normalized;

                    myTransform.position = simulation.GetGroundAt(Vector3.Lerp(initialPosition, destination, t));

                    var lookRotation = (targetPosition - myTransform.position).ElementWiseMultiply(ignoreHeight);
                    if (lookRotation == Vector3.zero)
                        return;

                    var targetRotation = Quaternion.LookRotation(lookRotation);
                    myTransform.rotation = Quaternion.Lerp(initialRotation, targetRotation, t * DEFAULT_TURN_SPEED);
                }
            ) {
                duration = Vector3.Distance(initialPosition, to.value) / DEFAULT_SPEED,
            };
        }
    }
}
