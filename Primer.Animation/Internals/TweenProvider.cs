using System;

namespace Primer.Animation
{
    public class TweenProvider : IEquatable<TweenProvider>
    {
        private readonly Tween constant;
        private readonly Func<Tween> getter;

        public Tween value => getter is not null ? getter() : constant;

        private TweenProvider(Tween tween) => constant = tween ?? throw new ArgumentNullException(nameof(tween));

        public TweenProvider(Func<Tween> getter) => this.getter = getter ?? throw new ArgumentNullException(nameof(getter));

        public override string ToString()
        {
            return getter is not null
                ? $"TweenProvider.Getter({getter()})"
                : $"TweenProvider.Value({constant})";
        }

        public static implicit operator TweenProvider(Tween val) => new(val);

        public static implicit operator TweenProvider(Func<Tween> val) => new(val);

        public static implicit operator Tween(TweenProvider @this) => @this.value;


        #region Equality
        public bool Equals(TweenProvider other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Nullable.Equals(constant, other.constant) && Equals(getter, other.getter);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return Equals((TweenProvider)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(constant, getter);
        }
        #endregion
    }
}
