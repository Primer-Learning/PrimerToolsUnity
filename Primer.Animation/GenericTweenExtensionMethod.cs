using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace Primer.Animation
{
    public static class GenericTweenExtensionMethod
    {
        #region target.Tween("member", to: 10)
        public static Tween Tween<T>(this object target, string member, T to, Func<T, T, float, T> lerp = null)
        {
            var accessors = GetAccessors<T>(target, member);
            return CreateTween(accessors, to, accessors.get(), lerp);
        }

        public static Tween Tween<T>(this object target, string member, T to, T from, Func<T, T, float, T> lerp = null)
        {
            var accessors = GetAccessors<T>(target, member);
            return CreateTween(accessors, to, from, lerp);
        }

        public static Tween Tween<T>(this object target, string member, Func<T, T> to, Func<T, T, float, T> lerp = null)
        {
            var accessors = GetAccessors<T>(target, member);
            var from = accessors.get();
            return CreateTween(accessors, to(from), from, lerp);
        }
        #endregion


        #region target.Tween(x => x.member, to: 10)
        public static Tween Tween<TContainer, TValue>(this TContainer target,
            Expression<Func<TContainer, TValue>> member, TValue to, Func<TValue, TValue, float, TValue> lerp = null)
        {
            var accessors = GetAccessors(target, member);
            return CreateTween(accessors, to, accessors.get(), lerp);
        }

        public static Tween Tween<TContainer, TValue>(this TContainer target,
            Expression<Func<TContainer, TValue>> member, Func<TValue, TValue> to, Func<TValue, TValue, float, TValue> lerp = null)
        {
            var accessors = GetAccessors(target, member);
            var from = accessors.get();
            return CreateTween(accessors, to(from), from, lerp);
        }

        public static Tween Tween<TContainer, TValue>(this TContainer target,
            Expression<Func<TContainer, TValue>> member, TValue to, TValue from,
            Func<TValue, TValue, float, TValue> lerp = null)
        {
            var accessors = GetAccessors(target, member);
            return CreateTween(accessors, to, from, lerp);
        }
        #endregion


        /// <summary>The actual implementation of the tween operation</summary>
        private static Tween CreateTween<T>(Accessors<T> member, T to, T from, Func<T, T, float, T> lerp = null)
        {
            lerp ??= GetLerpMethod<T>();
            return new Tween(t => member.set(lerp(from, to, t)));
        }


        #region Internals
        private static readonly Dictionary<Type, MethodInfo> lerpMethods = new();

        private static Func<T, T, float, T> GetLerpMethod<T>()
        {
            if (!lerpMethods.TryGetValue(typeof(T), out var method)) {
                method = GetNewLerpMethod<T>();
                lerpMethods.Add(typeof(T), method);
            }

            return (Func<T, T, float, T>)method.CreateDelegate(typeof(Func<T, T, float, T>));
        }

        private static MethodInfo GetNewLerpMethod<T>()
        {
            if (typeof(T) == typeof(float) || typeof(T) == typeof(int) || typeof(T) == typeof(double))
                return typeof(Mathf).GetMethod("Lerp");

            // Special case for Quaternion so we use Slerp instead of Lerp
            if (typeof(T) == typeof(Quaternion))
                return typeof(Quaternion).GetMethod("Slerp");

            var lerp = typeof(T).GetMethod("Lerp");

            if (lerp is null) {
                throw new ArgumentException($"{nameof(Tween)}() couldn't find static .Lerp() in {typeof(T).FullName}");
            }

            return lerp;
        }

        private static Accessors<TValue> GetAccessors<TContainer, TValue>(TContainer target,
            Expression<Func<TContainer, TValue>> member)
        {
            var expression = member.Body as MemberExpression;

            if (expression?.Member is FieldInfo field)
                return new Accessors<TValue>(target, field);

            if (expression?.Member is PropertyInfo property)
                return new Accessors<TValue>(target, property);

            throw new ArgumentException($"Member expression {member} not found in {target.GetType()}");
        }

        private static Accessors<T> GetAccessors<T>(object target, string member)
        {
            var type = target.GetType();

            var field = type.GetField(member);

            if (field is not null)
                return new Accessors<T>(target, field);

            var property = type.GetProperty(member);

            if (property is not null)
                return new Accessors<T>(target, property);

            throw new ArgumentException($"Member {member} not found in {type}");
        }

        private sealed class Accessors<T>
        {
            public readonly Action<T> set;
            public readonly Func<T> get;

            public Accessors(object target, FieldInfo field)
            {
                set = value => field.SetValue(target, value);
                get = () => (T)field.GetValue(target);
            }

            public Accessors(object target, PropertyInfo prop)
            {
                set = value => prop.SetValue(target, value);
                get = () => (T)prop.GetValue(target);
            }
        }
        #endregion
    }
}
