using System;
using System.Linq.Expressions;

namespace Primer
{
    // From https://stackoverflow.com/questions/1402803/passing-properties-by-reference-in-c-sharp/73762917#73762917
    // thanks @stoj and @Sven!

    public class Accessor<T>
    {
        private readonly Action<T> setter;
        private readonly Func<T> getter;

        public Accessor(Expression<Func<T>> expression)
        {
            if (expression.Body is not MemberExpression memberExpression)
                throw new ArgumentException("expression must return a field or property");

            var parameterExpression = Expression.Parameter(typeof(T));
            setter = Expression.Lambda<Action<T>>(Expression.Assign(memberExpression, parameterExpression), parameterExpression).Compile();
            getter = expression.Compile();
        }

        public void Set(T value) => setter(value);
        public T Get() => getter();
    }
}
