using System;
using UnityEngine;

namespace Primer
{
    /// <summary>
    ///    ChildrenDeclaration will ensure provided transform has all the required children and no more.
    /// </summary>
    ///
    /// <remarks>
    ///    It'll look it's current children for some matching that configuration (in this case only the name)
    ///
    ///    Any missing child will be created as new GameObject()
    ///    - can add components with `Next&lt;PrimerBehaviour&gt;()`
    ///    - or instance prefabs with `NextIsInstanceOf(prefab)`
    ///
    ///    Any extra child will be removed, onRemove callback available on constructor.
    /// </remarks>
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
        public int count => after.Count;

        /// <summary>Start a new child declaration</summary>
        /// <param name="parent">Transform to contain the children</param>
        /// <param name="onCreate">Callback to be invoked whenever a child is created</param>
        /// <param name="onRemove">Callback to be invoked whenever a child is removed</param>
        public ChildrenDeclaration(
            Transform parent,
            Action<Component> onCreate = null,
            Action<Transform> onRemove = null
        ) {
            DontCallMeOnPrefabException.ThrowIfIsPrefab(parent, nameof(ChildrenDeclaration));

            this.parent = parent;
            this.onCreate = onCreate;
            this.onRemove = onRemove ?? DefaultOnRemove;

            Reset();
        }

        /// <summary>
        ///     Forgets any previous declaration.
        ///     It leaves this instance of <c>ChildrenDeclaration</c> fresh to be used again.
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
        /// <param name="child">Component to be set as next child of <c>target</c></param>
        public void NextIs(Component child) => NextIs(child.transform);

        /// <summary>Next child is the component passed as argument</summary>
        /// <param name="child">Transform to be set as next child of <c>target</c></param>
        public void NextIs(Transform child)
        {
            after.Add(child);
            remaining.Remove(child);
        }


        /// <summary>
        ///     Forces the next call to <c>Next()</c> or <c>NextIsInstanceOf()</c>
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

            foreach (var remove in remaining)
                onRemove(remove);

            for (var i = 0; i < after.Count; i++) {
                var child = after[i];
                var pos = child.localPosition;
                var scale = child.localScale;

                needsReorder |= child.GetSiblingIndex() != i;

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
