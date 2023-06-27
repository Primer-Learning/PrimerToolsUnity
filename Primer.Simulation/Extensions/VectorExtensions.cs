using UnityEngine;

namespace Primer.Simulation
{
    public static class VectorExtensions
    {
        /// <summary>Simply cast to a 2D position from 3D position.</summary>
        /// <param name="position">A 3D relative-to-terrain position.</param>
        /// <returns>A 2D relative-to-terrain position.</returns>
        /// <remarks>Notice that no coordinate-space transformation is performed.</remarks>
        public static Vector2 To2D(this Vector3 position) => new(position.x, position.z);
        public static Vector2Int To2D(this Vector3Int position) => new(position.x, position.z);
    }
}
