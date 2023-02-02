using System;
using System.Text.RegularExpressions;

namespace Primer.Timeline
{
    [Serializable]
    internal struct MethodInvocation
    {
        private static readonly Regex uppercase = new("[A-Z]", RegexOptions.Compiled);

        public string methodName;

        public Type expectedReturnType;


        public object Invoke(object target, params object[] parameters)
        {
            var method = target.GetType().GetMethod(methodName);

            if (method is null) {
                throw new MethodAccessException(
                    $"Cannot find method {ToString(target)}"
                );
            }

            if (expectedReturnType != null && expectedReturnType.IsAssignableFrom(method.ReturnType)) {
                throw new MethodAccessException(
                    $"Method {ToString(target)} returns {method.ReturnType}, expected {expectedReturnType}"
                );
            }

            if (method.ReturnType != typeof(void))
                return method.Invoke(target, parameters);

            method.Invoke(target, parameters);
            return null;
        }


        public override string ToString()
        {
            return methodName is null
                ? null
                : uppercase.Replace(methodName, " $0").Trim();
        }

        public string ToString(object target)
        {
            var method = $"{methodName}()";

            return target == null
                ? method
                : $"{target.GetType().Name}.{method}";
        }
    }
}
