using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Simulation
{
    /// <summary>Ensures an object is always on the surface of a RandomElevationTerrain.</summary>
    [ExecuteInEditMode]
    public class TerrainMovement : MonoBehaviour
    {
        private RandomElevationTerrain terrainCache;
        private RandomElevationTerrain terrain => transform.ParentComponent(ref terrainCache);

        /// <summary>Default speed of object in local units per second.</summary>
        public float defaultSpeed = 10;
        /// <summary>Default turn speed of object in units of 2π (1 is 360°, 2 is 720°, etc).</summary>
        public float defaultTurnSpeed = 8;

        /// <summary>The 2D position of the object relative to the terrain.</summary>
        public Vector2 position2D => To2DPosition(transform, terrain);

        private void Update()
        {
            if (terrain)
                transform.localPosition = To3DPosition(position2D);
        }

        /// <summary>Get 2D position of an object.</summary>
        /// <param name="transform">The transform of the object.</param>
        /// <param name="terrain">The terrain the 2D position should be relative to.</param>
        /// <returns>A 2D relative-to-terrain position.</returns>
        public static Vector2 To2DPosition(Transform transform, RandomElevationTerrain terrain)
        {
            return terrain.transform.InverseTransformPoint(transform.position).To2D();
        }

        /// <summary>Get 3D position from a 2D position.</summary>
        /// <param name="position">A 2D relative-to-terrain position.</param>
        /// <returns>
        ///     A relative-to-our-parent 3D position (ie: we could set `this.transform.localPosition` to
        ///     this value and we'd be in the right spot).
        /// </returns>
        private Vector3 To3DPosition(Vector2 position)
        {
            var point = terrain.GetGroundAt(position.x, position.y);
            return transform.parent.InverseTransformPoint(point);
        }

        public record Step
        {
            public Vector2 position2D { get; init; }
            public Quaternion rotation { get; init; }

            public Vector2 targetPosition2D { get; init; }
            public Quaternion targetRotation { get; init; }

            public float maxTraversalDelta { get; init; }
            public float maxRotationDelta { get; init; }
        }

        public Step GetStepTowards(Component to, float? speed = null, float? turnSpeed = null)
            => GetStepTowards(new TransformTarget(to.transform), speed, turnSpeed);

        public Step GetStepTowards(Vector2 to, float? speed = null, float? turnSpeed = null)
            => GetStepTowards(new Vector2Target(to), speed, turnSpeed);

        private Step GetStepTowards(Target to, float? speed, float? turnSpeed)
        {
            speed ??= defaultSpeed;
            turnSpeed ??= defaultTurnSpeed;

            var deltaTime = Time.deltaTime;

            var ourPosition = position2D;
            var theirPosition = to.GetPosition(this);

            var maxRotationDelta = 360f * turnSpeed.Value * deltaTime;
            var maxTraversalDelta = speed.Value * deltaTime;

            // Get our desired "forward" direction (where we'll be looking). To avoid rotating
            // the blob so its tilted back (ie: we only ever want to rotate round the y axis)
            // we pretend our target is on the same y-fixed plane with us.
            var our3DPosition = terrain.transform.InverseTransformPoint(transform.position);
            var their3DPosition = to.Get3DPosition(this);
            var desiredForward = terrain.transform.TransformVector(
                new Vector3(their3DPosition.x, our3DPosition.y, their3DPosition.z) - our3DPosition
            );

            var rotation = transform.rotation;
            var targetQuaternion = transform.rotation;

            if (desiredForward != Vector3.zero) {
                targetQuaternion = Quaternion.LookRotation(desiredForward, Vector3.up);
                rotation = Quaternion.RotateTowards(transform.rotation, targetQuaternion, maxRotationDelta);
            }

            var distance = Vector2.Distance(ourPosition, theirPosition);
            var direction = (theirPosition - ourPosition).normalized;

            return new Step {
                position2D = ourPosition + direction * Mathf.Min(distance, maxTraversalDelta),
                rotation = rotation,
                targetPosition2D = theirPosition,
                targetRotation = targetQuaternion,
                maxTraversalDelta = maxTraversalDelta,
                maxRotationDelta = maxRotationDelta,
            };
        }

        /// <summary>Moves object to given target.</summary>
        /// <param name="to">The target to move to.</param>
        /// <param name="speed">The speed of movement. Defaults to `defaultSpeed`.</param>
        /// <param name="turnSpeed">
        ///     The speed of turning. Defaults to `defaultTurnSpeed` (and is in the same
        ///     units).
        /// </param>
        /// <param name="cancellationToken">
        ///     The move will be cancelled if this `cancellationToken` is
        ///     cancelled.
        /// </param>
        /// <remarks>The task is completed when we've moved to the target.</remarks>
        public UniTask MoveTo(Component to, CancellationToken cancellationToken = default)
            => MoveTo(new TransformTarget(to.transform), ct: cancellationToken);

        /// <summary>Moves object to given target.</summary>
        /// <param name="to">The target to move to.</param>
        /// <param name="speed">The speed of movement. Defaults to `defaultSpeed`.</param>
        /// <param name="turnSpeed">
        ///     The speed of turning. Defaults to `defaultTurnSpeed` (and is in the same
        ///     units).
        /// </param>
        /// <param name="cancellationToken">
        ///     The move will be cancelled if this `cancellationToken` is
        ///     cancelled.
        /// </param>
        /// <remarks>The task is completed when we've moved to the target.</remarks>
        public UniTask MoveTo(Vector2 to, CancellationToken cancellationToken = default)
            => MoveTo(new Vector2Target(to), ct: cancellationToken);

        private async UniTask MoveTo(Target to, float? speed = null, float? turnSpeed = null,
            CancellationToken ct = default)
        {
            var mergedToken = CancellationTokenSource
                .CreateLinkedTokenSource(ct, this.GetCancellationTokenOnDestroy())
                .Token;

            while (true) {
                var step = GetStepTowards(to, speed, turnSpeed);

                // Get the distance from our targets. We'll compare this against the most we could
                // have moved this step to know whether we reached our target.
                var distance = Vector2.Distance(position2D, step.targetPosition2D);

                var rotationDistance =
                    Mathf.Abs(Quaternion.Angle(transform.rotation, step.targetRotation));

                transform.localPosition = To3DPosition(step.position2D);
                transform.rotation = step.rotation;

                await UniTask.NextFrame(mergedToken);

                if (distance <= step.maxTraversalDelta && rotationDistance <= step.maxRotationDelta)
                    return;
            }
        }

        /// <summary>Move instantaneously to a point.</summary>
        /// <param name="to">A 2D relative-to-terrain position.</param>
        public void TeleportTo(Vector2 to)
        {
            transform.localPosition = To3DPosition(to);
        }

        private static Vector2 RandomVector(Vector2 min, Vector2 max)
        {
            return new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
        }

        #region Target class
        private abstract class Target
        {
            public abstract Vector2 GetPosition(TerrainMovement source);
            public abstract Vector3 Get3DPosition(TerrainMovement source);
        }

        private class Vector2Target : Target
        {
            private readonly Vector2 target;
            public Vector2Target(Vector2 target) => this.target = target;
            public override Vector2 GetPosition(TerrainMovement source) => target;
            public override Vector3 Get3DPosition(TerrainMovement source) => new(target.x, 0, target.y);
        }

        private class TransformTarget : Target
        {
            private readonly Transform target;
            public TransformTarget(Transform target) => this.target = target;

            public override Vector2 GetPosition(TerrainMovement source)
            {
                return To2DPosition(target.transform, source.terrain);
            }

            public override Vector3 Get3DPosition(TerrainMovement source)
            {
                return source.terrain.transform.InverseTransformPoint(target.transform.position);
            }
        }
        #endregion
    }
}
