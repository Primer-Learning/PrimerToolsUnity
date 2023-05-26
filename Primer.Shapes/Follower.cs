using System;
using UnityEngine;

namespace Primer.Shapes
{
    [ExecuteAlways]
    public class Follower : MonoBehaviour
    {
        public bool useGlobalSpace = false;
        public Func<Vector3> getter;

        private void LateUpdate()
        {
            if (getter is null)
                return;

            if (useGlobalSpace)
                transform.position = getter();
            else
                transform.localPosition = getter();
        }
    }
}
