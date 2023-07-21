using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public class Vector3Comparer : IEqualityComparer<Vector3>
    {
        private float tolerance;

        public Vector3Comparer(float tolerance)
        {
            this.tolerance = tolerance;
        }

        public bool Equals(Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, b) < tolerance;
        }

        public int GetHashCode(Vector3 obj)
        {
            // Compute a hash code in a way that respects the tolerance.
            // This is a simple example and might need to be adapted depending on your specific needs.
            return obj.x.GetHashCode() ^ obj.y.GetHashCode() ^ obj.z.GetHashCode();
        }
    }
}