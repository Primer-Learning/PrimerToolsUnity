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

        public Landscape landscape
        {
            get => _landscape ? _landscape : transform.GetComponentInParent<Landscape>();
            set => _landscape = value;
        }


        public float distanceFromBorder = 0;
        public bool center = true;

        [Title("Spawn items")]
        public float minDistance = 2;
        [FormerlySerializedAs("amount")] public int numberToPlace = 30;
        public int maxAttemptsPerPoint = 30;
        public PoissonDiscSampler.OverflowMode overflowMode = PoissonDiscSampler.OverflowMode.None;
        public bool randomizeRotation = true;
        public Transform prefab;

        #region public Vector2 size;
        [SerializeField, HideInInspector]
        [FormerlySerializedAs("size")]
        private Vector2 _size = new(100, 100);

        [Title("Spawn area")]
        [ShowInInspector]
        [PropertyOrder(-1)]
        [DisableIf(nameof(landscape))]
        public Vector2 size {
            get => landscape ? new Vector2(landscape.size.x, landscape.size.y) : _size;
            set => _size = value;
        }
        #endregion

        [Title("Controls")]
        public int seed;
        public bool incrementSeed;
        
        [Button(ButtonSizes.Large)]
        // [ButtonGroup]
        [DisableIf(nameof(locked))]
        public void Place()
        {
            if (incrementSeed)
                seed++;
            
#if UNITY_EDITOR
            if (locked) {
                throw new Exception(
                    $"Refusing to replace children while locked. Unlock {name}'s {nameof(PoissonPrefabPlacer)} to continue."
                );
            }
#endif

            var hasLandscape = landscape != null;
            var gnome = new Gnome(transform);
            var spawnSpace = size - Vector2.one * distanceFromBorder * 2;
            var offset = Vector2.one * distanceFromBorder;

            if (center)
                offset -= size / 2;

            var rng = new Rng(seed);

            foreach (var point in PoissonDiscSampler.Rectangular(numberToPlace, spawnSpace, minDistance, overflowMode: overflowMode, rng: rng, numSamplesBeforeRejection: maxAttemptsPerPoint)) {
                var instance = gnome.Add(prefab);
                var pos = point + offset;

                if (hasLandscape) {
                    instance.localPosition = landscape.GetGroundAtLocal(pos);
                    instance.GetOrAddComponent<LandscapeItem>();
                } else {
                    instance.localPosition = new Vector3(pos.x, 0, pos.y);
                }
                
                // Make sure the objects are visible in the editor.
                // A sequence can make them scale 0 when necessary, but we always want to see them when 
                // hitting the button.
                if (instance.localScale == Vector3.zero) {instance.localScale = Vector3.one;}

                if (randomizeRotation)
                    instance.localRotation = Quaternion.Euler(0, rng.Range(0, 360), 0);
            }

            gnome.Purge();
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
