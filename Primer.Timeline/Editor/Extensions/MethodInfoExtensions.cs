using System;
using System.CodeDom;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;

namespace Primer.Timeline.Editor
{
    public static class MethodInfoExtensions
    {
        private static readonly CSharpCodeProvider provider = new CSharpCodeProvider();

        private static string NameOf(Type type)
            => provider.GetTypeOutput(new CodeTypeReference(type)).Split('.').Last();


        public static string Print(this MethodInfo method)
        {
            var returnType = NameOf(method.ReturnType);
            var parameters = method.GetParameters().Select(x => NameOf(x.ParameterType));
            return $"{returnType} {method.Name}({string.Join(", ", parameters)})";
        }
    }
}
