using System;
using UnityEngine;

namespace Primer
{
    public partial class ChildrenDeclaration
    {
        #region T a = children.NextIsInstanceOf(prefab)
        /// <summary>Next child is instantiated from <pre>prefab</pre></summary>
        /// <param name="prefab">Prefab to instantiate</param>
        /// <param name="cache">
        ///    Pass a variable or field to save a reference to the child once created.
        ///    This saves ChildrenDeclaration from having to find the child from the target's children.
        /// </param>
        /// <param name="name">GameObject's name</param>
        /// <param name="init">Initializer function, will be invoked only when the child is created</param>
        /// <typeparam name="T">Type of the prefab. No need to define it as it will be inferred</typeparam>
        /// <returns>The copy created from the prefab</returns>
        public T NextIsInstanceOf<T>(T prefab, ref T cache, string name = null, Action<T> init = null)
            where T : Component
        {
            return UseCache(cache) ?? (cache = NextIsInstanceOf(prefab, name, init));
        }

        /// <summary>Next child is instantiated from <pre>prefab</pre></summary>
        /// <param name="prefab">Prefab to instantiate</param>
        /// <param name="name">GameObject's name</param>
        /// <param name="init">Initializer function, will be invoked only when the child is created</param>
        /// <typeparam name="T">Type of the prefab. No need to define it as it will be inferred</typeparam>
        /// <returns>The copy created from the prefab</returns>
        public T NextIsInstanceOf<T>(T prefab, string name = null, Action<T> init = null) where T : Component
        {
            if (Find(out var found, name, prefab))
                return Add(found);

            var instance = InstantiationTracker.InstantiateAndRegister(prefab, parent);
            Initialize(instance, name, init);
            return Add(instance);
        }
        #endregion


        #region Something a = children.NextIsInstanceOf(prefab, init: x => x.AddComponent<Something>());
        /// <summary>
        ///     Next child is instantiated from <pre>prefab</pre> and the initializer converts it to type <pre>TCached</pre>
        /// </summary>
        /// <param name="prefab">Prefab to instantiate</param>
        /// <param name="cache">
        ///    Pass a variable or field to save a reference to the child once created.
        ///    This saves ChildrenDeclaration from having to find the child from the target's children.
        /// </param>
        /// <param name="name">GameObject's name</param>
        /// <param name="init">Initializer function, will be invoked only when the child is created</param>
        /// <typeparam name="TPrefab">Type of the prefab. Inferred from <pre>prefab</pre> parameter</typeparam>
        /// <typeparam name="TCached">Type returned by initializer. Inferred from <pre>init</pre> parameter</typeparam>
        /// <returns>The value returned by <pre>init</pre></returns>
        public TCached NextIsInstanceOf<TPrefab, TCached>(
            TPrefab prefab,
            ref TCached cache,
            // In this case in particular `init` comes before `name` because it's mandatory
            Func<TPrefab, TCached> init,
            string name = null)
            where TPrefab : Component
            where TCached : Component
        {
            return UseCache(cache) ?? (cache = NextIsInstanceOf(prefab, init, name));
        }

        /// <summary>
        ///     Next child is instantiated from <pre>prefab</pre> and the initializer converts it to type <pre>TCached</pre>
        /// </summary>
        /// <param name="prefab">Prefab to instantiate</param>
        /// <param name="name">GameObject's name</param>
        /// <param name="init">Initializer function, will be invoked only when the child is created</param>
        /// <typeparam name="TPrefab">Type of the prefab. Inferred from <pre>prefab</pre> parameter</typeparam>
        /// <typeparam name="TCached">Type returned by initializer. Inferred from <pre>init</pre> parameter</typeparam>
        /// <returns>The value returned by <pre>init</pre></returns>
        public TCached NextIsInstanceOf<TPrefab, TCached>(
            TPrefab prefab,
            // In this case in particular `init` comes before `name` because it's mandatory
            Func<TPrefab, TCached> init,
            string name = null)
            where TPrefab : Component
            where TCached : Component
        {
            if (Find(out Transform foundTransform, name, prefab)) {
                var found = foundTransform.GetComponent<TCached>();

                if (found is not null)
                    return Add(found);
            }

            var instance = InstantiationTracker.InstantiateAndRegister(prefab, parent);
            Initialize(instance, name);

            var result = init(instance);
            return Add(result);
        }
        #endregion


        #region Hidden Component used to track which prefab was used to instantiate a given component
        [DisallowMultipleComponent]
        internal class InstantiationTracker : MonoBehaviour
        {
            public static bool IsInstanceOf(Component target, Component prefab)
            {
                var tracker = target.GetComponent<InstantiationTracker>();
                return tracker is not null && (tracker.from == IdentifyPrefab(prefab));
            }

            public static T InstantiateAndRegister<T>(T prefab, Transform parent, bool worldPositionStays = false)
                where T : Component
            {
                var target = Instantiate(prefab, parent, worldPositionStays);
                var tracker = target.gameObject.AddComponent<InstantiationTracker>();
                tracker.hideFlags = HideFlags.HideInInspector;
                tracker.from = IdentifyPrefab(prefab);
                return target;
            }

            private static string IdentifyPrefab(Component prefab) => prefab.gameObject.name;


            public string from;
        }
        #endregion
    }
}
