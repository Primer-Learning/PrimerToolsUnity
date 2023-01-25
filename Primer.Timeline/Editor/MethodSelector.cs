using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
using UnityEditor;
using UnityEngine;

namespace Primer.Timeline.Editor
{
    public static class MethodSelector
    {
        private static readonly string[] ignoreMethods = {
            "IsInvoking",
            "CancelInvoke",
            "StopAllCoroutines",
            "GetComponent",
            "GetComponentInChildren",
            "GetComponentsInChildren",
            "GetComponentInParent",
            "GetComponentsInParent",
            "GetComponents",
            "GetInstanceID",
            "GetHashCode",
            "ToString",
            "GetType",
            "Equals",
        };

        public static string Render(string currentValue, MethodInfo[] methods, bool hideIfEmpty = false)
        {
            if (methods.Length == 0)
                return null;

            if (methods.Length == 1 && hideIfEmpty)
                return methods[0].Name;

            var options = methods.Select(x => x.Name).ToArray();

            EditorGUI.BeginDisabledGroup(options.Length == 1);
            var labels = methods.Select(x => x.Print()).ToArray();
            var index = Mathf.Clamp(Array.IndexOf(options, currentValue), 0, options.Length);
            var newIndex = EditorGUILayout.Popup("Method to invoke", index, labels);
            EditorGUI.EndDisabledGroup();

            return options[newIndex];
        }

        public static MethodInfo[] GetMethodsWithNoParams<T>(T value)
        {
            var methods = MethodsWithNoParams(value).ToArray();

            if (methods.Length == 0) {
                EditorGUILayout.HelpBox(
                    $"Couldn't find methods without parameters in {value.GetType().FullName}",
                    MessageType.Error
                );
            }

            return methods;
        }

        private static IEnumerable<MethodInfo> MethodsWithNoParams<T>(T value)
        {
            foreach (var method in GetMethods(value)) {
                var parameters = method.GetParameters();

                if (parameters.All(param => param.IsOptional))
                    yield return method;
            }
        }

        public static MethodInfo[] GetMethodsWithParamsOfType<T>(T value, params Type[] types)
        {
            var methods = MethodsWithParamsOfType(value, types).ToArray();

            if (methods.Length == 0) {
                EditorGUILayout.HelpBox(
                    $"Couldn't find methods without parameters in {value.GetType().FullName}",
                    MessageType.Error
                );
            }

            return methods;
        }

        private static IEnumerable<MethodInfo> MethodsWithParamsOfType<T>(T value, params Type[] types)
        {
            foreach (var method in GetMethods(value)) {
                if (ParameterTypeMatches(method.GetParameters(), types))
                    yield return method;
            }
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

        private static IEnumerable<MethodInfo> GetMethods<T>(T value)
        {
            return value.GetType()
                .GetMethods()
                .Where(x => !x.Name.StartsWith("get_") && !ignoreMethods.Contains(x.Name));
        }
    }
}
