using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Simulation
{
    [ExecuteAlways]
    public class PoissonPrefabPlacer : MonoBehaviour
    {
        public Landscape landscape;
        [DisableIf(nameof(landscape))]
        public Vector2 size = new(100, 100);
        public float distanceFromBorder = 0;
        public float minDistance = 2;
        public int amount = 30;
        public PrefabProvider prefab;
        public bool center = true;
        public bool randomizeRotation = true;

        public void OnEnable()
        {
            if (!landscape)
                landscape = GetComponentInParent<Landscape>() ?? GetComponentInParent<ISimulation>()?.terrain;

            if (landscape)
                size = new Vector2(landscape.size.x, landscape.size.z);
        }

        public void SetPrefabByName(string prefabName)
        {
            prefab ??= new PrefabProvider();
            prefab.value = Prefab.Get(prefabName);
        }

        public void EmplaceIfEmpty(int? amount = null, string prefabName = null)
        {
            if (prefabName is not null)
                SetPrefabByName(prefabName);

            if (amount is not null)
                this.amount = amount.Value;

            if (transform.childCount != this.amount)
                Emplace();
        }

        [Button]
        public void Emplace()
        {
            if (prefab?.value is null)
                throw new Exception("Need a prefab to place!");

            var hasLandscape = landscape != null;
            var container = new Container(transform);
            var spawnSpace = size - Vector2.one * distanceFromBorder * 2;
            var offset = Vector2.one * distanceFromBorder;

            if (center)
                offset -= size / 2;

            foreach (var point in PoissonDiscSampler.Rectangular(amount, spawnSpace, minDistance)) {
                var instance = container.Add(prefab);
                var pos = point + offset;

                if (hasLandscape) {
                    instance.localPosition = landscape.GetGroundAtLocal(pos);
                    instance.GetOrAddComponent<LandscapeItem>();
                } else {
                    instance.localPosition = new Vector3(pos.x, 0, pos.y);
                }

                if (randomizeRotation)
                    instance.localRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
            }

            container.Purge();
        }
    }
}
