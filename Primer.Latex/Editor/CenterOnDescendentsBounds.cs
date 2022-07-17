using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.LatexRenderer
{
    public static class CenterOnDescendentsBounds
    {
        /// <summary>
        ///     Equivalent to parent.GetComponentsInChildren but only returns components in descendents
        ///     (not any components attached to parent directly).
        /// </summary>
        private static List<T> GetComponentsInJustDescendents<T>(Transform parent)
        {
            List<T> result = new();
            foreach (Transform immediateChild in parent)
                result.AddRange(immediateChild.GetComponentsInChildren<T>());

            return result;
        }

        /// <summary>Calculates the smallest Bounds object that contains all given Bounds objects.</summary>
        private static Bounds GetSuperBounds(IEnumerable<Bounds> allBounds)
        {
            var min = Vector3.positiveInfinity;
            var max = Vector3.negativeInfinity;
            foreach (var bounds in allBounds)
            {
                min = Vector3.Min(min, bounds.min);
                max = Vector3.Max(max, bounds.max);
            }

            var result = new Bounds();
            result.SetMinMax(min, max);
            return result;
        }

        [MenuItem("GameObject/Primer Learning/Center on Descendents' Bounds", false, 1)]
        private static void CenterOnChildren()
        {
            var selected = Selection.gameObjects[0];
            var renderers = GetComponentsInJustDescendents<Renderer>(selected.transform);
            var childrenBounds = GetSuperBounds(renderers.Select(i => i.bounds));
            var childrenToPosition = renderers
                .Select(renderer => (renderer.gameObject, renderer.transform.position))
                .ToDictionary(i => i.gameObject, i => i.position);

            Undo.RecordObject(selected.transform, "");
            selected.transform.position = childrenBounds.center;
            foreach (var renderer in renderers)
            {
                Undo.RecordObject(renderer.transform, "");
                renderer.transform.position = childrenToPosition[renderer.gameObject];
            }

            Undo.SetCurrentGroupName("Center on Descendents' Bounds");
        }

        [MenuItem("GameObject/Primer Learning/Center on Descendents' Bounds", true, 1)]
        private static bool CenterOnChildrenValidation()
        {
            if (Selection.gameObjects.Length != 1) return false;

            var selected = Selection.gameObjects[0];
            return GetComponentsInJustDescendents<Renderer>(selected.transform).Count > 0;
        }
    }
}