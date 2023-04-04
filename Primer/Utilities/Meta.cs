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

        // Uncomment in case of need
        //
        // public static bool ReactiveProp<T>(ref T field, T newValue, Action<T, T> onChange, Action hook = null)
        // {
        //     var oldValue = field;
        //
        //     if (!ReactiveProp(ref field, newValue))
        //         return false;
        //
        //     onChange?.Invoke(newValue, oldValue);
        //     hook?.Invoke();
        //     return true;
        // }

        public static bool ReactiveProp<T>(Expression<Func<T>> expression, T newValue)
        {
            var getter = Accessor<T>.Getter(expression);

            if (Equals(getter(), newValue))
                return false;

            var setter = Accessor<T>.Setter(expression);
            setter(newValue);
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

        // Uncomment in case of need
        //
        // public static bool ReactiveProp<T>(Expression<Func<T>> expression, T newValue, Action<T, T> onChange, Action hook = null)
        // {
        //     var getter = Accessor<T>.Getter(expression);
        //     var prevValue = getter();
        //
        //     if (Equals(prevValue, newValue))
        //         return false;
        //
        //     var setter = Accessor<T>.Setter(expression);
        //     setter(newValue);
        //     onChange?.Invoke(newValue, prevValue);
        //     hook?.Invoke();
        //     return true;
        // }
    }
}
