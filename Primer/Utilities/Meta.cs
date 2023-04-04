using System;
using System.Linq.Expressions;

namespace Primer
{
    public static class Meta
    {
        public static bool ReactiveProp<T>(ref T field, T newValue)
        {
            if (Equals(field, newValue))
                return false;

            field = newValue;
            return true;
        }

        public static bool ReactiveProp<T>(ref T field, T newValue, Action onChange, Action hook = null)
        {
            if (!ReactiveProp(ref field, newValue))
                return false;

            onChange?.Invoke();
            hook?.Invoke();
            return true;
        }

        public static bool ReactiveProp<T>(ref T field, T newValue, Action<T> onChange, Action hook = null)
        {
            if (!ReactiveProp(ref field, newValue))
                return false;

            onChange?.Invoke(newValue);
            hook?.Invoke();
            return true;
        }

        public static bool ReactiveProp<T>(ref T field, T newValue, Action<T, T> onChange, Action hook = null)
        {
            var oldValue = field;

            if (!ReactiveProp(ref field, newValue))
                return false;

            onChange?.Invoke(newValue, oldValue);
            hook?.Invoke();
            return true;
        }

        public static bool ReactiveProp<T>(Expression<Func<T>> expression, T newValue)
        {
            var getter = expression.Compile();

            if (Equals(getter(), newValue))
                return false;

            var accessor = new Accessor<T>(expression);
            accessor.Set(newValue);
            return true;
        }

        public static bool ReactiveProp<T>(Expression<Func<T>> expression, T newValue, Action onChange, Action hook = null)
        {
            if (!ReactiveProp(expression, newValue))
                return false;

            onChange?.Invoke();
            hook?.Invoke();
            return true;
        }

        public static bool ReactiveProp<T>(Expression<Func<T>> expression, T newValue, Action<T> onChange, Action hook = null)
        {
            if (!ReactiveProp(expression, newValue))
                return false;

            onChange?.Invoke(newValue);
            hook?.Invoke();
            return true;
        }

        public static bool ReactiveProp<T>(Expression<Func<T>> expression, T newValue, Action<T, T> onChange, Action hook = null)
        {
            var getter = expression.Compile();

            if (Equals(getter(), newValue))
                return false;

            var accessor = new Accessor<T>(expression);
            accessor.Set(newValue);
            onChange?.Invoke(newValue, accessor.Get());
            hook?.Invoke();
            return true;
        }
    }
}
