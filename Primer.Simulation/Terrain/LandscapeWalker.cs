using Primer.Animation;
using Primer.Shapes;
using UnityEngine;

namespace Primer.Simulation
{
    public class LandscapeWalker : MonoBehaviour
    {
        private const float DEFAULT_SPEED = 4;
        private const float DEFAULT_TURN_SPEED = 16;

        private Landscape terrainCache;
        private Landscape terrain => transform.ParentComponent(ref terrainCache);

        public Tween WalkTo(Vector2 to) => WalkToImpl(terrain.GetGroundAt(to));
        public Tween WalkToLocal(Vector2 to) => WalkToImpl(terrain.GetGroundAtLocal(to));

        public Tween WalkTo(Component to)
            => WalkToImpl(new Vector3Provider(() => terrain.GetGroundAt(to.transform.position)));

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

                    myTransform.position = terrain.GetGroundAt(Vector3.Lerp(initialPosition, destination, t));

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
