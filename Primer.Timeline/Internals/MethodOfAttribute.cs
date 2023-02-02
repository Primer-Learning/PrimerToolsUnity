using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Primer.Timeline
{
    public class MethodOfAttribute : Attribute
    {

        public readonly string sourceProp;

        public Type returnType { get; init; }
        public Type[] parameters { get; init; }
        public string[] excludeNames { get; init; }


        public MethodOfAttribute(string target = null)
        {
            sourceProp = target;
        }

        public MethodInfo[] Filter(MethodInfo[] methods) //, object containerObject = null)
        {
            return methods.Where(
                x => {
                    if (returnType is not null && x.ReturnType != returnType)
                        return false;

                    if (parameters is not null && !ParameterTypeMatches(x.GetParameters(), parameters))
                        return false;

                    if (excludeNames is not null && excludeNames.Contains(x.Name))
                        return false;

                    return true;
                })
                .ToArray();
        }

        private static bool ParameterTypeMatches(IEnumerable<ParameterInfo> parameters, IEnumerable<Type> types)
        {
            var remaining = new Queue<Type>(types);

            foreach (var parameter in parameters) {
                if (remaining.Count == 0)
                    return parameter.IsOptional;

                if (parameter.IsIn || parameter.IsOut || parameter.IsRetval)
                    return false;

                var expected = remaining.Peek();

                if (expected == parameter.ParameterType || expected.IsSubclassOf(parameter.ParameterType))
                    remaining.Dequeue();
            }

            return remaining.Count == 0;
        }
    }
}
