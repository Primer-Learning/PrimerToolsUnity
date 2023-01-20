using System;
using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    /// <summary>
    ///    ChildrenDeclaration will ensure provided transform has all the required children and no more.
    ///    It'll look it's current children for some matching that configuration (in this case only the name)
    ///
    ///    Any missing child will be created as new GameObject()
    ///    - can add components with `Next&lt;PrimerBehaviour&gt;()`
    ///    - or instance prefabs with `NextIsInstanceOf(prefab)`
    ///
    ///    Any extra child will be removed, onRemove callback available on constructor.
    /// </summary>
    ///
    /// <example>
    ///     <code>
    ///         var children = new ChildrenDeclaration(transform);
    ///
    ///         for (var i = 0; i &lt; 10; i++)
    ///             children.Next($"Child {i}");
    ///
    ///         children.Apply();
    ///     </code>
    /// </example>
    public partial class ChildrenDeclaration : IChildrenDeclaration
    {
        /// <summary>Start a new child declaration</summary>
        /// <param name="parent">Transform to contain the children</param>
        /// <param name="onCreate">Callback to be invoked whenever a child is created</param>
        /// <param name="onRemove">Callback to be invoked whenever a child is removed</param>
        public ChildrenDeclaration(
            Transform parent,
            Action<Component> onCreate = null,
            Action<Transform> onRemove = null)
        {
            DontCallMeOnPrefabException.ThrowIfIsPrefab(parent, nameof(ChildrenDeclaration));

            this.parent = parent;
            this.onCreate = onCreate;
            this.onRemove = onRemove ?? DefaultOnRemove;

            Reset();
        }

        /// <summary>
        ///     Forgets any previous declaration.
        ///     It leaves this instance of <pre>ChildrenDeclaration</pre> fresh to be used again.
        /// </summary>
        public void Reset()
        {
            after.Clear();
            remaining.Clear();

            for (int i = 0, childCount = parent.childCount; i < childCount; i++) {
                var child = parent.GetChild(i);

                if (child != null)
                    remaining.Add(child);
            }
        }


        /// <summary>Next child is the component passed as argument</summary>
        /// <param name="target">Component to be set as next child of <pre>target</pre></param>
        public void NextIs(Component target) => NextIs(target.transform);

        /// <summary>Next child is the component passed as argument</summary>
        /// <param name="target">Transform to be set as next child of <pre>target</pre></param>
        public void NextIs(Transform target)
        {
            after.Add(target);
            remaining.Remove(target);
        }


        /// <summary>
        ///     Forces the next call to <pre>Next()</pre> or <pre>NextIsInstanceOf()</pre>
        ///     to ignore cache and existing children.
        ///     Thus re-initializing the next child.
        /// </summary>
        public void ReinitializeNextChild()
        {
            recreateNextChild = true;
        }


        /// <summary>
        ///     Executes the declaration.
        ///     Removes any present child that doesn't match the declaration.
        ///     And adds any declared child that wasn't previously preset.
        /// </summary>
        public void Apply()
        {
            var needsReorder = false;

            foreach (var remove in remaining) {
                onRemove(remove);

                if (remove != null)
                    remove.parent = null;
            }

            foreach (var child in after) {
                var pos = child.localPosition;
                var scale = child.localScale;

                if (child.parent == parent) {
                    if (needsReorder)
                        child.parent = null;
                    else
                        continue;
                }

                child.parent = parent;
                child.localPosition = pos;
                child.localScale = scale;
                needsReorder = true;
            }
        }
    }
}
