using System;
using System.Linq.Expressions;

namespace Primer
{
    // From https://stackoverflow.com/questions/1402803/passing-properties-by-reference-in-c-sharp/73762917#73762917
    // thanks @stoj and @Sven!

    public class Accessor<T>
    {
        private readonly MemberExpression memberExpression;
        private readonly Func<T> getter;
        private Action<T> setter;

        public Accessor(Expression<Func<T>> expression)
        {
            if (expression.Body is not MemberExpression member)
                throw new ArgumentException("expression must return a field or property");

            getter = expression.Compile();
            memberExpression = member;
        }

        private Action<T> CreateSetter()
        {
            var parameterExpression = Expression.Parameter(typeof(T));
            var assignExpression = Expression.Assign(memberExpression, parameterExpression);
            var lambda = Expression.Lambda<Action<T>>(assignExpression, parameterExpression);
            return lambda.Compile();
        }

        public T Get()
        {
            return getter();
        }

        public void Set(T value)
        {
            setter ??= CreateSetter();
            setter(value);
        }
    }
}
