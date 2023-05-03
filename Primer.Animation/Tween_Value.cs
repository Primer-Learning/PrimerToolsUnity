using System;
using System.Linq.Expressions;

namespace Primer.Animation
{
    public partial record Tween
    {
        public static Tween Value<T>(Expression<Func<T>> expression, T to, Func<T, T, float, T> lerp = null)
        {
            var accessor = new Accessor<T>(expression);
            return CreateTween(accessor, accessor.Get(), to, lerp);
        }

        public static Tween Value<T>(Expression<Func<T>> expression, T from, T to, Func<T, T, float, T> lerp = null)
        {
            var accessor = new Accessor<T>(expression);
            return CreateTween(accessor, from, to, lerp);
        }

        public static Tween Value<T>(Expression<Func<T>> expression, Func<T, T> to, Func<T, T, float, T> lerp = null)
        {
            var accessor = new Accessor<T>(expression);
            return CreateTween(accessor, accessor.Get(), to(accessor.Get()), lerp);
        }

        public static Tween Value<T>(Expression<Func<T>> expression, T from, Func<T, T> to, Func<T, T, float, T> lerp = null)
        {
            var accessor = new Accessor<T>(expression);
            return CreateTween(accessor, from, to(accessor.Get()), lerp);
        }

        /// <summary>The actual implementation of the tween operation</summary>
        private static Tween CreateTween<T>(Accessor<T> accessor, T from, T to, Func<T, T, float, T> lerp)
        {
            lerp ??= LerpMethods.GetLerpMethod<T>();
            return new Tween(t => accessor.Set(lerp(from, to, t)));
        }
    }
}
