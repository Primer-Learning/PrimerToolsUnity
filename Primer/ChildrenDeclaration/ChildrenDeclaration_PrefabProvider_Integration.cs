using System;
using UnityEngine;

// This warning is thrown because we didn't document parameters
// - this ChildrenDeclaration declaration
// Those parameters are not meant to be used by the user, but by the compiler
// ReSharper disable InvalidXmlDocComment

namespace Primer
{
    public static class ChildrenDeclaration_PrefabProvider_Integration
    {
        #region T a = children.NextIsInstanceOf(provider)
        /// <summary>Next child is instantiated from <pre>prefab</pre></summary>
        /// <param name="provider">Prefab to instantiate</param>
        /// <param name="cache">
        ///    Pass a variable or field to save a reference to the child once created.
        ///    This saves ChildrenDeclaration from having to find the child from the target's children.
        /// </param>
        /// <param name="name">GameObject's name</param>
        /// <param name="init">Initializer function, will be invoked only when the child is created</param>
        /// <typeparam name="T">Type of the prefab. No need to define it as it will be inferred</typeparam>
        /// <returns>The copy created from the prefab</returns>
        public static T NextIsInstanceOf<T>(
            this IChildrenDeclaration declaration,
            PrefabProvider<T> provider,
            ref T cache,
            string name = null,
            Action<T> init = null)
            where T : Component
        {
            if (provider.value == null)
                throw new EmptyProviderException();

            return declaration.NextIsInstanceOf(
                prefab: provider.value,
                ref cache,
                name,
                init: value => {
                    provider.Initialize(value);
                    init?.Invoke(value);
                }
            );
        }

        /// <summary>Next child is instantiated from <pre>prefab</pre></summary>
        /// <param name="provider">Prefab to instantiate</param>
        /// <param name="name">GameObject's name</param>
        /// <param name="init">Initializer function, will be invoked only when the child is created</param>
        /// <typeparam name="T">Type of the prefab. No need to define it as it will be inferred</typeparam>
        /// <returns>The copy created from the prefab</returns>
        public static T NextIsInstanceOf<T>(
            this IChildrenDeclaration declaration,
            PrefabProvider<T> provider,
            string name = null,
            Action<T> init = null)
            where T : Component
        {
            if (provider.value == null)
                throw new EmptyProviderException();

            return declaration.NextIsInstanceOf(
                prefab: provider.value,
                name,
                init: value => {
                    provider.Initialize(value);
                    init?.Invoke(value);
                }
            );
        }
        #endregion


        #region Something a = children.NextIsInstanceOf(prefab, init: x => x.AddComponent<Something>());
        /// <summary>
        ///     Next child is instantiated from <pre>prefab</pre> and the initializer converts it to type <pre>TCached</pre>
        /// </summary>
        /// <param name="provider">Prefab to instantiate</param>
        /// <param name="cache">
        ///    Pass a variable or field to save a reference to the child once created.
        ///    This saves ChildrenDeclaration from having to find the child from the target's children.
        /// </param>
        /// <param name="name">GameObject's name</param>
        /// <param name="init">Initializer function, will be invoked only when the child is created</param>
        /// <typeparam name="TPrefab">Type of the prefab. Inferred from <pre>prefab</pre> parameter</typeparam>
        /// <typeparam name="TCached">Type returned by initializer. Inferred from <pre>init</pre> parameter</typeparam>
        /// <returns>The value returned by <pre>init</pre></returns>
        public static TCached NextIsInstanceOf<TPrefab, TCached>(
            this ChildrenDeclaration declaration,
            PrefabProvider<TPrefab> provider,
            ref TCached cache,
            // In this case in particular `init` comes before `name` because it's mandatory
            Func<TPrefab, TCached> init,
            string name = null)
            where TPrefab : Component
            where TCached : Component
        {
            if (provider.value == null)
                throw new EmptyProviderException();

            return declaration.NextIsInstanceOf(
                prefab: provider.value,
                ref cache,
                name: name,
                init: value => {
                    provider.Initialize(value);
                    return init.Invoke(value);
                }
            );
        }

        /// <summary>
        ///     Next child is instantiated from <pre>prefab</pre> and the initializer converts it to type <pre>TCached</pre>
        /// </summary>
        /// <param name="provider">Prefab to instantiate</param>
        /// <param name="name">GameObject's name</param>
        /// <param name="init">Initializer function, will be invoked only when the child is created</param>
        /// <typeparam name="TPrefab">Type of the prefab. Inferred from <pre>prefab</pre> parameter</typeparam>
        /// <typeparam name="TCached">Type returned by initializer. Inferred from <pre>init</pre> parameter</typeparam>
        /// <returns>The value returned by <pre>init</pre></returns>
        public static TCached NextIsInstanceOf<TPrefab, TCached>(
            this ChildrenDeclaration declaration,
            PrefabProvider<TPrefab> provider,
            // In this case in particular `init` comes before `name` because it's mandatory
            Func<TPrefab, TCached> init,
            string name = null)
            where TPrefab : Component
            where TCached : Component
        {
            if (provider.value == null)
                throw new EmptyProviderException();

            return declaration.NextIsInstanceOf(
                prefab: provider.value,
                name: name,
                init: value => {
                    provider.Initialize(value);
                    return init.Invoke(value);
                }
            );
        }
        #endregion
    }
}
