using System;
using System.Text.RegularExpressions;

namespace Primer.Timeline
{
    [Serializable]
    internal class MethodInvocation
    {
        private static readonly Regex uppercase = new("[A-Z]", RegexOptions.Compiled);

        public string methodName;

        public void Invoke(object target, params object[] parameters)
        {
            var method = target.GetType().GetMethod(methodName);

            if (method is null) {
                throw new MethodAccessException(
                    $"Cannot find method {methodName} on {target.GetType().FullName}"
                );
            }

            method.Invoke(target, parameters);
        }

        public override string ToString()
            => uppercase.Replace(methodName, " $0").Trim();
    }
}
