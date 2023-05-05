using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

namespace Primer
{
    // From https://stackoverflow.com/questions/1402803/passing-properties-by-reference-in-c-sharp/73762917#73762917
    // thanks @stoj and @Sven!

    public class Accessor<T>
    {
        public static Func<T> Getter(Expression<Func<T>> expression)
        {
            return expression.Compile();
        }

        public static Action<T> Setter(Expression<Func<T>> expression)
        {
            if (expression.Body is not MemberExpression memberExpression)
                throw new ArgumentException("expression must return a field or property");

            var parameterExpression = Expression.Parameter(typeof(T));
            var assignExpression = Expression.Assign(memberExpression, parameterExpression);
            var lambda = Expression.Lambda<Action<T>>(assignExpression, parameterExpression);
            return lambda.Compile();
        }

        private readonly string sourceFile;
        private readonly int lineNumber;
        private readonly MemberExpression memberExpression;
        private readonly Func<T> getter;
        private Action<T> setter;

        public Accessor(Expression<Func<T>> expression, string sourceFile = "Unknown", int lineNumber = 0)
        {
            if (expression.Body is not MemberExpression member)
                throw new ArgumentException("expression must return a field or property");

            this.sourceFile = sourceFile;
            this.lineNumber = lineNumber;
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
            try {
                return getter();
            }
            catch (Exception ex) {
                LogError("getting", ex);
                throw;
            }
        }

        public void Set(T value)
        {
            setter ??= CreateSetter();
            try {
                setter(value);
            }
            catch (Exception ex) {
                LogError("setting", ex);
                throw;
            }
        }

        private void LogError(string method, Exception ex)
        {
            var filename = sourceFile.Split(Path.PathSeparator).Last();
            var link = $"<a href=\"{sourceFile}\" line=\"{lineNumber}\">{filename}:{lineNumber}</a>";
            Debug.LogError($"Error {method} value at {link}\n\nInternal error below\n\n{ex.Message}");
        }
    }
}
