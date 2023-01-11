using System;
using UnityEngine;

namespace Primer
{
    public interface IChildrenDeclaration
    {
        public void NextIs(Transform target);

        public void NextIs(Component target);

        public Transform Next(
            string name = null,
            Action<Transform> init = null
        );

        public Transform Next(
            ref Transform cache,
            string name = null,
            Action<Transform> init = null
        );

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
            Func<TPrefab, TCached> init,
            string name = null
        )
            where TPrefab : Component
            where TCached : Component;

        public TCached NextIsInstanceOf<TPrefab, TCached>(
            TPrefab prefab,
            ref TCached cache,
            Func<TPrefab, TCached> init,
            string name = null
        )
            where TPrefab : Component
            where TCached : Component;

        public void Apply();
    }
}
