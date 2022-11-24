using System.Collections.Generic;
using UnityEditor;

namespace Primer.Latex
{
    public static class SerializedPropertyExtensions
    {
        public static List<string> GetStringArrayValue(this SerializedProperty array) {
            var result = new List<string>();

            for (var i = 0; i < array.arraySize; i++) {
                result.Add(array.GetArrayElementAtIndex(i).stringValue);
            }

            return result;
        }

        public static void SetStringArrayValue(this SerializedProperty array, IReadOnlyList<string> value) {
            array.ClearArray();

            for (var i = 0; i < value.Count; i++) {
                array.InsertArrayElementAtIndex(i);
                array.GetArrayElementAtIndex(i).stringValue = value[i];
            }
        }
    }
}
