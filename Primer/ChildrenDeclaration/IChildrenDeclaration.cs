using System;
using UnityEngine;

namespace Primer
{
    public interface IChildrenDeclaration
    {
        /// <summary>
        ///     Updates the target's children list to match
        /// </summary>
        public void Apply();

        public void ReinitializeNextChild();


        public void NextIs(Transform target);

        public void NextIs(Component target);


        // following methods follow this pattern
        // method definition
        public Transform Next(string name = null, Action<Transform> init = null);
        // then the same method with `ref cache` parameter (refs can't be optional)
        public Transform Next(ref Transform cache, string name = null, Action<Transform> init = null);


        public TComponent Next<TComponent>(
            string name = null,
            Action<TComponent> init = null
        ) where TComponent : Component;

        public TComponent Next<TComponent>(
            ref TComponent cache,
            string name = null,
            Action<TComponent> init = null
        ) where TComponent : Component;


        public TComponent NextIsInstanceOf<TComponent>(
            TComponent prefab,
            string name = null,
            Action<TComponent> init = null
        ) where TComponent : Component;

        public TComponent NextIsInstanceOf<TComponent>(
            TComponent prefab,
            ref TComponent cache,
            string name = null,
            Action<TComponent> init = null
        ) where TComponent : Component;


        public TCached NextIsInstanceOf<TPrefab, TCached>(
            TPrefab prefab,
            // In this case in particular `init` comes before `name` because it's mandatory
            Func<TPrefab, TCached> init,
            string name = null
        )
            where TPrefab : Component
            where TCached : Component;

        public TCached NextIsInstanceOf<TPrefab, TCached>(
            TPrefab prefab,
            ref TCached cache,
            // In this case in particular `init` comes before `name` because it's mandatory
            Func<TPrefab, TCached> init,
            string name = null
        )
            where TPrefab : Component
            where TCached : Component;
    }
}
