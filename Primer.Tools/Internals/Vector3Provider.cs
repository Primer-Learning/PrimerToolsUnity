using System;
using UnityEngine;

namespace Primer.Tools
{
    public class Vector3Provider
    {
        private readonly Vector3? constant;
        private readonly Func<Vector3> func;
        private readonly Transform source;

        public Vector3 value {
            get {
                if (constant.HasValue)
                    return constant.Value;

                if (func is not null)
                    return func();

                return source.position;
            }
        }

        private Vector3Provider(Vector3 vector3) => constant = vector3;

        public Vector3Provider(Func<Vector3> getter) => func = getter;

        private Vector3Provider(Transform follow) => source = follow;

        public static implicit operator Vector3Provider(Vector3 val) => new(val);

        public static implicit operator Vector3Provider(Func<Vector3> val) => new(val);

        public static implicit operator Vector3Provider(Transform val) => new(val);

        public static implicit operator Vector3(Vector3Provider @this) => @this.value;
    }
}
