using System.Collections.Generic;
using UnityEditor;

namespace Primer
{
    public static class SerializedPropertyExtensions
    {
        public static IEnumerable<SerializedProperty> GetChildProperties(this SerializedProperty property)
        {
            var sibling = property.Copy();
            sibling.NextVisible(false);

            var iterator = property.Copy();

            while (iterator.NextVisible(true)) {
                // We reached the sibling
                if (iterator.propertyPath == sibling.propertyPath)
                    break;

                yield return iterator;
            }
        }

        public static List<int> GetIntArrayValue(this SerializedProperty array)
        {
            var result = new List<int>();

            for (var i = 0; i < array.arraySize; i++) {
                result.Add(array.GetArrayElementAtIndex(i).intValue);
            }

            return result;
        }


        public static void SetIntArrayValue(this SerializedProperty array, IReadOnlyList<int> value)
        {
            array.ClearArray();

            for (var i = 0; i < value.Count; i++) {
                array.InsertArrayElementAtIndex(i);
                array.GetArrayElementAtIndex(i).intValue = value[i];
            }
        }
    }
}
