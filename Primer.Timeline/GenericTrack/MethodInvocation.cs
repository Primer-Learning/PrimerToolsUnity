using System;
using System.Text.RegularExpressions;

namespace Primer.Timeline
{
    [Serializable]
    internal struct MethodInvocation
    {
        private static readonly Regex uppercase = new("[A-Z]", RegexOptions.Compiled);

        public string methodName;

        public bool isReady => !string.IsNullOrEmpty(methodName);

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
            => methodName is null
                ? null
                : uppercase.Replace(methodName, " $0").Trim();
    }
}
