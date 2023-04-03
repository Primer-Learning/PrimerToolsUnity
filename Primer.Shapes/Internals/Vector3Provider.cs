using System;
using UnityEngine;

namespace Primer.Shapes
{
    public class Vector3Provider : IEquatable<Vector3Provider>
    {
        private readonly Vector3? constant;
        private readonly Func<Vector3> getter;
        private readonly Transform followGlobal;
        private readonly Transform followLocal;

        public Vector3 value
        {
            get
            {
                if (constant.HasValue)
                    return constant.Value;

                if (getter is not null)
                    return getter();

                return followGlobal is null
                    ? followLocal.localPosition
                    : followGlobal.position;
            }
        }

        public void ApplyTo(ScenePoint point)
        {
            if (constant.HasValue)
                point.vector = constant.Value;
            else if (getter is not null)
                point.getter = getter;
            else
            {
                point.follow = followLocal ?? followGlobal;
                point.isWorldPosition = followGlobal is not null;
            }
        }

        private Vector3Provider(Vector3 vector3) => constant = vector3;

        public Vector3Provider(Func<Vector3> getter) => this.getter = getter;

        public Vector3Provider(Transform follow, bool useWorldPosition = true)
        {
            if (useWorldPosition)
                followGlobal = follow;
            else
                followLocal = follow;
        }

        public override string ToString()
        {
            if (constant.HasValue)
                return $"Vector3Provider.Value({constant.Value})";

            if (getter is not null)
                return $"Vector3Provider.Getter({getter()})";

            return followGlobal is null
                ? $"Vector3Provider.Follow(local\"{followLocal.name}\", {followLocal.localPosition})"
                : $"Vector3Provider.Follow(global\"{followGlobal.name}\", {followGlobal.position})";
        }

        public static implicit operator Vector3Provider(Vector3 val) => new(val);

        public static implicit operator Vector3Provider(Func<Vector3> val) => new(val);

        public static implicit operator Vector3Provider(Transform val) => new(val);

        public static implicit operator Vector3(Vector3Provider @this) => @this.value;


        #region Equality
        public bool Equals(Vector3Provider other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Nullable.Equals(constant, other.constant)
                && Equals(getter, other.getter)
                && Equals(followGlobal, other.followGlobal)
                && Equals(followLocal, other.followLocal);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return Equals((Vector3Provider)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(constant, getter, followGlobal, followLocal);
        }
        #endregion
    }
}
