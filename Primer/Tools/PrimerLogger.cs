using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Primer
{
    public static class PrimerLogger
    {
        private const int BREAK_IF_LONGER_THAN = 100;
        private static int indentation;

        private static string indent => "".PadLeft(indentation * 4, ' ');


        public static void Log(Component component, params object[] data)
            => Log(ComponentLabel(component), data);

        public static void Log(params object[] data) => Log("", data);

        public static void Log(string label, params object[] data)
            => Debug.Log($"{label} {string.Join(' ', data.Select(Print))}");

        public static void Error(Component component, Exception exception)
            => Debug.LogError($"{ComponentLabel(component)} {exception.Message}");

        public static void Error(Exception exception)
            => Debug.LogError(exception);

        private static string ComponentLabel(Component component)
        {
            var type = component.GetType().Name;
            var gameObject = component.gameObject;
            var name = gameObject.name;
            var isPrefab = gameObject.IsPreset() ? " (prefab)" : "";
            var parent = component.transform.parent;
            var parentName = parent == null ? "" : $"{parent.gameObject.name} > ";
            return $"{type}{isPrefab} [{parentName}{name}]";
        }


        private static string Print(bool target) => target.ToString();
        private static string Print(int target) => target.ToString();
        private static string Print(float target) => target.ToString(CultureInfo.InvariantCulture);
        private static string Print(double target) => target.ToString(CultureInfo.InvariantCulture);
        private static string Print(decimal target) => target.ToString(CultureInfo.InvariantCulture);
        private static string Print(string target) => $"\"{target}\"";

        private static string Print(object target)
        {
            return target switch {
                null => "null",
                bool x => Print(x),
                int x => Print(x),
                float x => Print(x),
                double x => Print(x),
                decimal x => Print(x),
                string x => Print(x),
                IEnumerable<bool> x => PrintList(() => x.Select(Print)),
                IEnumerable<int> x => PrintList(() => x.Select(Print)),
                IEnumerable<float> x => PrintList(() => x.Select(Print)),
                IEnumerable<double> x => PrintList(() => x.Select(Print)),
                IEnumerable<decimal> x => PrintList(() => x.Select(Print)),
                IEnumerable<string> x => PrintList(() => x),
                IEnumerable<object> x => PrintList(() => x.Select(Print)),
                IEnumerable x => PrintList(() => x.Cast<object>().Select(Print)),
                _ => target.ToString(),
            };
        }

        private static string PrintList(Func<IEnumerable<string>> getter)
        {
            var array = getter().ToArray();
            var length = array.Sum(x => x.Length);

            if (length < BREAK_IF_LONGER_THAN)
                return $"[({array.Length}) {string.Join(", ", array)} ]";

            indentation++;
            var innerIndent = indent;
            var content = string.Join($",\n{innerIndent}", getter().ToArray());
            indentation--;

            return $"[({array.Length})\n{innerIndent}{content}\n{indent}]";
        }
    }
}
