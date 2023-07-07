using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Simulation
{
    [ExecuteAlways]
    public class PoissonPrefabDistribution : MonoBehaviour
    {
        public Landscape landscape;
        [DisableIf(nameof(landscape))]
        public Vector2 size = new(100, 100);
        public float minDistance = 2;
        public int amount = 30;
        public PrefabProvider prefab;
        public bool center;

        public void OnEnable()
        {
            if (!landscape)
                landscape = GetComponentInParent<ISimulation>()?.terrain;

            if (landscape)
                size = new Vector2(landscape.size.x, landscape.size.z);
        }

        [Button]
        public void Emplace()
        {
            if (prefab?.value is null)
                throw new Exception("Need a prefab to place!");

            var offset = center ? size / -2 : Vector2.zero;
            var container = new Container(transform);

            Func<Vector2, Vector3> toVector3 = landscape
                ? v => landscape.GetGroundAtLocal(v)
                : v => new Vector3(v.x, 0, v.y);

            foreach (var point in PoissonDiscSampler.Rectangular(amount, size, minDistance)) {
                var instance = container.Add(prefab);
                instance.localPosition = toVector3(point + offset);
            }

            container.Purge();
        }
    }
}
