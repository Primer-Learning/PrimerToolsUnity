using System;
using UnityEngine;

namespace Primer
{
    public partial class ChildrenDeclaration
    {
        /// <summary>
        ///     Removes all children from <pre>parent</pre>
        /// </summary>
        /// <param name="parent">Transform to leave without children</param>
        /// <param name="onRemove">Callback to be executed for every removed child</param>
        public static void Clear(Component parent, Action<Transform> onRemove = null)
            => Clear(parent.transform, onRemove);

        /// <summary>
        ///     Removes all children from <pre>parent</pre>
        /// </summary>
        /// <param name="parent">Transform to leave without children</param>
        /// <param name="onRemove">Callback to be executed for every removed child</param>
        public static void Clear(Transform parent, Action<Transform> onRemove = null)
        {
            var remove = onRemove ?? DefaultOnRemove;
            var childCount = parent.childCount;
            var children = new Transform[childCount];

            for (var i = 0; i < childCount; i++)
                children[i] = parent.GetChild(i);

            for (var i = 0; i < childCount; i++) {
                remove(children[i]);

                if (children[i] != null)
                    children[i].parent = null;
            }
        }
    }
}
