using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;

namespace Primer.Animation
{
    // Tween.Value() allows to tween an arbitrary field, property or variable from A to B.
    //
    //    var tween = Tween.Value(() => transform.position, Vector3.zero, Vector3.one);
    //
    // Be aware:
    // If the value we're trying to tween is a readonly field or a property with only a getter but no setter
    // we'll get a runtime error. Since we have no way detecting that at compile time.
    //
    public partial record Tween
    {
        public static Tween Value<T>(Expression<Func<T>> expression, T to, Func<T, T, float, T> lerp = null,
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            var accessor = new Accessor<T>(expression, filePath, lineNumber);
            return CreateTween(accessor.Set, accessor.Get, () => to, lerp);
        }

        public static Tween Value<T>(Expression<Func<T>> expression, T from, T to, Func<T, T, float, T> lerp = null,
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            var accessor = new Accessor<T>(expression, filePath, lineNumber);
            return CreateTween(accessor.Set, () => from, () => to, lerp);
        }

        public static Tween Value<T>(Expression<Func<T>> expression, Func<T, T> to, Func<T, T, float, T> lerp = null,
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            var accessor = new Accessor<T>(expression, filePath, lineNumber);
            return CreateTween(accessor.Set, accessor.Get, () => to(accessor.Get()), lerp);
        }

        public static Tween Value<T>(Expression<Func<T>> expression, T from, Func<T, T> to,
            Func<T, T, float, T> lerp = null, [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            var accessor = new Accessor<T>(expression, filePath, lineNumber);
            return CreateTween(accessor.Set, () => from, () => to(accessor.Get()), lerp);
        }

        public static Tween Value<T>(Action<T> set, Func<T> from, Func<T> to, Func<T, T, float, T> lerp = null)
        {
            return CreateTween(set, from, to, lerp);
        }

        /// <summary>The actual implementation of the tween operation</summary>
        private static Tween CreateTween<T>(Action<T> set, Func<T> getFrom, Func<T> getTo, Func<T, T, float, T> lerp)
        {
            lerp ??= LerpMethods.GetLerpMethod<T>();

            var initialized = false;
            var from = default(T);
            var to = default(T);

            return new Tween(
                t => {
                    if (!initialized) {
                        from = getFrom();
                        to = getTo();
                        initialized = true;
                    }

                    set(lerp(from, to, t));
                }
            );
        }
        
        // These overloads allow duration to be determined when the tween is first evaluated.
        public static Tween Value<T>(Action<T> set, Func<T> from, Func<T> to, Func<float> duration, Func<T, T, float, T> lerp = null)
        {
            return CreateTween(set, from, to, duration, lerp);
        }

        /// <summary>The actual implementation of the tween operation</summary>
        private static Tween CreateTween<T>(Action<T> set, Func<T> getFrom, Func<T> getTo, Func<float> getDuration, Func<T, T, float, T> lerp)
        {
            lerp ??= LerpMethods.GetLerpMethod<T>();

            var initialized = false;
            var from = default(T);
            var to = default(T);

            return new Tween(
                    t =>
                    {
                        if (!initialized)
                        {
                            from = getFrom();
                            to = getTo();
                            initialized = true;
                        }

                        set(lerp(from, to, t));
                    }
                )
                {
                    durationFunc = getDuration
                };
        }
    }
}
