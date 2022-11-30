using System.Collections.Generic;
using UnityEditor;

namespace DefaultNamespace
{
    public static class SerializedPropertyExtensions
    {
        public static IEnumerable<SerializedProperty> GetChildProperties(this SerializedProperty property) {
            var sibling = property.Copy();
            sibling.NextVisible(false);

            var iterator = property.Copy();

            while (iterator.NextVisible(true)) {
                // We reached the sibling
                if (iterator.propertyPath == sibling.propertyPath) break;
                yield return iterator;
            }
        }
    }
}
