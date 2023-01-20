using System;
using UnityEngine;

namespace Primer
{
    public partial class ChildrenDeclaration
    {
        #region Transform a = children.Next()
        /// <summary>Next child is a simple GameObject</summary>
        /// <param name="cache">
        ///    Pass a variable or field to save a reference to the child once created.
        ///    This saves ChildrenDeclaration from having to find the child from the target's children.
        /// </param>
        /// <param name="name">GameObject's name</param>
        /// <param name="init">Initializer function, will be invoked only when the child is created</param>
        /// <returns>The GameObject's Transform component</returns>
        public Transform Next(ref Transform cache, string name = null, Action<Transform> init = null)
            => UseCache(cache) ?? (cache = Next(name, init));

        /// <summary>Next child is a simple GameObject</summary>
        /// <param name="name">GameObject's name</param>
        /// <param name="init">Initializer function, will be invoked only when the child is created</param>
        /// <returns>The GameObject's Transform component</returns>
        public Transform Next(string name = null, Action<Transform> init = null)
        {
            if (Find(out var found, name))
                return Add(found);

            var transform = new GameObject().transform;
            Initialize(transform, name, init);
            return Add(transform);
        }
        #endregion


        #region Something a = children.Next<Something>();
        /// <summary>Next child is a GameObject with <pre>T</pre> component</summary>
        /// <param name="cache">
        ///    Pass a variable or field to save a reference to the child once created.
        ///    This saves ChildrenDeclaration from having to find the child from the target's children.
        /// </param>
        /// <param name="name">GameObject's name</param>
        /// <param name="init">Initializer function, will be invoked only when the child is created</param>
        /// <typeparam name="T">The component to add to the child</typeparam>
        /// <returns>The component of type <pre>T</pre> added to the child</returns>
        public T Next<T>(ref T cache, string name = null, Action<T> init = null) where T : Component
            => UseCache(cache) ?? (cache = Next(name, init));

        /// <summary>Next child is a GameObject with <pre>T</pre> component</summary>
        /// <param name="name">GameObject's name</param>
        /// <param name="init">Initializer function, will be invoked only when the child is created</param>
        /// <typeparam name="T">The component to add to the child</typeparam>
        /// <returns>The component of type <pre>T</pre> added to the child</returns>
        public T Next<T>(string name = null, Action<T> init = null) where T : Component
        {
            if (Find<T>(out var found, name))
                return Add(found);

            var component = new GameObject().AddComponent<T>();
            Initialize(component, name, init);
            return Add(component);
        }
        #endregion
    }
}
