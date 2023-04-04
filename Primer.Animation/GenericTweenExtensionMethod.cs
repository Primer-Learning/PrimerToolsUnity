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
            lerp ??= LerpMethods.GetLerpMethod<T>();
            return new Tween(t => member.set(lerp(from, to, t)));
        }


        #region Internals
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
