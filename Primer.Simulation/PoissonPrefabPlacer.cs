using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Primer.Simulation
{
    [ExecuteAlways]
    public class PoissonPrefabPlacer : MonoBehaviour
    {
        [SerializeField]
        private Landscape _landscape;
        public Landscape landscape => this.FindTerrain(ref _landscape);


        public float distanceFromBorder = 0;
        public bool center = true;

        [Title("Spawn items")]
        public float minDistance = 2;
        public int amount = 30;
        public bool randomizeRotation = true;
        public PrefabProvider prefab;

        #region public Vector2 size;
        [SerializeField, HideInInspector]
        [FormerlySerializedAs("size")]
        private Vector2 _size = new(100, 100);

        [Title("Spawn area")]
        [ShowInInspector]
        [PropertyOrder(-1)]
        [DisableIf(nameof(landscape))]
        public Vector2 size {
            get => landscape ? new Vector2(landscape.size.x, landscape.size.z) : _size;
            set => _size = value;
        }
        #endregion

        public void SetPrefabByName(string prefabName)
        {
            prefab ??= new PrefabProvider();
            prefab.value = Prefab.Get(prefabName);
        }

        public void EmplaceIfEmpty(string prefabName = null)
        {
            if (prefabName is not null)
                SetPrefabByName(prefabName);

            if (transform.childCount is 0)
                Emplace();
        }

        [Title("Controls")]
        [Button(ButtonSizes.Large)]
        // [ButtonGroup]
        [DisableIf(nameof(locked))]
        public void Emplace()
        {
            if (prefab?.value is null)
                throw new Exception("Need a prefab to place!");

#if UNITY_EDITOR
            if (locked) {
                throw new Exception(
                    $"Refusing to replace children while locked. Unlock {name}'s {nameof(PoissonPrefabPlacer)} to continue."
                );
            }
#endif

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

#if UNITY_EDITOR
        [SerializeField, HideInInspector]
        private bool locked = false;

        [PropertySpace]
        [Button(ButtonSizes.Medium, Icon = SdfIconType.Lock)]
        // [ButtonGroup]
        [HideIf(nameof(locked))]
        private void Lock() => locked = true;

        [PropertySpace]
        [Button(ButtonSizes.Medium, Icon = SdfIconType.Unlock)]
        // [ButtonGroup]
        [ShowIf(nameof(locked))]
        private void Unlock() => locked = false;
#endif
    }
}
