using UnityEngine;

namespace Primer.Extensions
{
    public static class RigidbodyExtensions
    {
        public static void AddForceScaled(this Rigidbody rb, Vector3 force, Vector3 scale)
        {
            var scaleFactor = (scale.x + scale.y + scale.z) / 3;
            var scaledForce = force * Mathf.Sqrt(scaleFactor);
            rb.AddForce(scaledForce);
        }
    }
}
