using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Primer.Timeline
{
    [Serializable]
    public struct NoBullshitExposedReference<T> where T : Object
    {
        public readonly ExposedReference<T> exposedReference;

        public NoBullshitExposedReference(ExposedReference<T> exposedReference)
        {
            this.exposedReference = exposedReference;
        }

        // Convert to ExposedReference
        public static implicit operator ExposedReference<T>(NoBullshitExposedReference<T> noBullshit)
        {
            return noBullshit.exposedReference;
        }

        // Create from string
        public static uint counter = 0;
        public static implicit operator NoBullshitExposedReference<T>(string name)
        {
            return new NoBullshitExposedReference<T>(
                new ExposedReference<T> {
                    exposedName = $"{name}{counter++}",
                }
            );
        }
    }
}
