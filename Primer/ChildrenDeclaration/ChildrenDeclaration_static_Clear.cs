using System;
using System.Linq;
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
        {
            Clear(parent.transform, onRemove);
        }

        /// <summary>
        ///     Removes all children from <pre>parent</pre>
        /// </summary>
        /// <param name="parent">Transform to leave without children</param>
        /// <param name="onRemove">Callback to be executed for every removed child</param>
        public static void Clear(Transform parent, Action<Transform> onRemove = null, Transform[] skip = null)
        {
            var remove = onRemove ?? DefaultOnRemove;
            var children = parent.GetChildren();

            for (var i = 0; i < children.Length; i++) {
                if (skip is not null && skip.Contains(children[i]))
                    continue;

                remove(children[i]);
            }
        }
    }
}
