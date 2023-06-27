using Primer.Animation;
using Primer.Shapes;
using UnityEngine;

namespace Primer.Simulation
{
    public class LandscapeWalker : MonoBehaviour
    {
        private const float DEFAULT_SPEED = 10;
        private const float DEFAULT_TURN_SPEED = 8;

        private Landscape terrainCache;
        private Landscape terrain => transform.ParentComponent(ref terrainCache);

        public Tween WalkTo(Component to)
        {
            return WalkToInternal(
                new Vector3Provider(
                    () => {
                        var pos = to.transform.position;
                        return terrain.GetGroundAt(pos.x, pos.z);
                    }
                )
            );
        }

        public Tween WalkTo(Vector2 to)
        {
            var destination = terrain.GetGroundAt(to.x, to.y);
            return WalkToInternal(destination);
        }

        private Tween WalkToInternal(Vector3Provider to, float? maybeSpeed = null, float? maybeTurnSpeed = null)
        {
            var speed = maybeSpeed ?? DEFAULT_SPEED;
            var turnSpeed = maybeTurnSpeed ?? DEFAULT_TURN_SPEED;
            var myTransform = transform;
            var initialPosition = myTransform.position;
            var initialRotation = myTransform.rotation;

            return new Tween(
                t => {
                    var targetPosition = to.value;
                    var targetRotation = Quaternion.LookRotation(targetPosition - myTransform.position);
                    myTransform.position = Vector3.Lerp(initialPosition, to.value, t);
                    myTransform.rotation = Quaternion.Lerp(initialRotation, targetRotation, t * turnSpeed);
                }
            ) {
                duration = Vector3.Distance(initialPosition, to.value) / speed,
            };
        }
    }
}
