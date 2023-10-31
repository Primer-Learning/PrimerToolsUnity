using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Primer.Simulation
{
    [ExecuteAlways]
    public class PoissonPrefabPlacer : MonoBehaviour
    {
        public Transform[] allChildren => transform.Find("Trees").GetChildren().Concat(transform.Find("Homes").GetChildren()).ToArray();
        
        [SerializeField]
        private Landscape _landscape;

        public Landscape landscape
        {
            get => _landscape ? _landscape : transform.GetComponentInParent<Landscape>();
            set => _landscape = value;
        }
        
        List<Vector2> points = new();
        private Vector2 oldSize;

        public float distanceFromBorder = 0;
        public bool center = true;
        public Vector2 offset;

        [Title("Spawn items")]
        public float minDistance = 2;
        public int numberToPlace1 = 30;
        public int numberToPlace2 = 0;
        public Transform prefab1;
        public Transform prefab2;
        public int maxAttemptsPerPoint = 30;
        public PoissonDiscSampler.OverflowMode overflowMode = PoissonDiscSampler.OverflowMode.None;
        public bool randomizeRotation = true;
        private List<bool> prefabAssignments;

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
            
            var prefab1Pool = new Pool<Transform>("Trees", parent: transform);
            prefab1Pool.prefab = prefab1.gameObject;
            prefab1Pool.Reset();
            var prefab2Pool = new Pool<Transform>("Homes", parent: transform);
            prefab2Pool.prefab = prefab2.gameObject;
            prefab2Pool.Reset();
            
            var spawnSpace = size - Vector2.one * distanceFromBorder * 2;

            var rng = new Rng(seed);
            
            // Make a list of bools that reflect the desired counts of each prefab
            prefabAssignments = Enumerable.Repeat(true, numberToPlace1)
                .Concat(Enumerable.Repeat(false, numberToPlace2)).Shuffle(rng: rng);
            
            var index = 0;
            
            points = PoissonDiscSampler.RectangularPointSet(numberToPlace1 + numberToPlace2, spawnSpace, minDistance,
                overflowMode: overflowMode, rng: rng, numSamplesBeforeRejection: maxAttemptsPerPoint).ToList();
            if (center)
            {
                offset = -points.Average();
                points = points.Select(x => x + offset).ToList();
            }
                
            oldSize = size;
            // Old implementation of centering. This pays attention to geometry, but doesn't consider the actual points.
            // The above implementation could push things out of bounds, but is unlikely to. It otherwise 
            // produces better-looking results.
            // {
            //     offset = Vector2.one * distanceFromBorder;
            //     offset -= size / 2;
            // }

            foreach (var point in points) {
                // Start here
                var pool = prefabAssignments[index++] ? prefab1Pool : prefab2Pool;
                var instance = pool.Add();
                
                var pos = point;

                if (hasLandscape) {
                    instance.position = landscape.GetGroundAtWorldPoint(landscape.transform.TransformPoint(new Vector3(pos.x, 0, pos.y)));
                    var landscapeItem = instance.GetOrAddComponent<LandscapeItem>();
                    landscapeItem.landscape = landscape;
                } else {
                    instance.localPosition = new Vector3(pos.x, 0, pos.y);
                }
                
                // Make sure the objects are visible in the editor.
                // A sequence can make them scale 0 when necessary, but we always want to see them when 
                // hitting the button.
                if (instance.localScale == Vector3.zero) {instance.localScale = Vector3.one;}

                if (randomizeRotation)
                    instance.localRotation = Quaternion.Euler(0, rng.RangeInt(0, 360), 0);
            }

            if (prefab1Pool.activeChildCount < numberToPlace1)
                Debug.LogWarning("Only " + prefab1Pool.activeChildCount + " trees were placed out of an attempted " + numberToPlace1 + ".");
            if (prefab2Pool.activeChildCount < numberToPlace2)
                Debug.LogWarning("Only " + prefab2Pool.activeChildCount + " homes were placed out of an attempted " + numberToPlace2 + ".");
        }

        [Button(ButtonSizes.Large)]
        // [ButtonGroup]
        [DisableIf(nameof(locked))]
        public void PlaceMore()
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
            var prefab1Pool = new Pool<Transform>("Trees", parent: transform);
            prefab1Pool.prefab = prefab1.gameObject;
            var prefab2Pool = new Pool<Transform>("Homes", parent: transform);
            prefab2Pool.prefab = prefab2.gameObject;
            var spawnSpace = size - Vector2.one * distanceFromBorder * 2;
            
            // offset = Vector2.one * distanceFromBorder;
            //
            // if (center)
            //     offset -= size / 2;
            // if (center) offset = -points.Average();

            var rng = new Rng(seed);
            
            var prefab1ToAdd = numberToPlace1 - prefabAssignments.Count(x => x);
            var prefab2ToAdd = numberToPlace2 - prefabAssignments.Count(x => !x);
            prefabAssignments = prefabAssignments.Concat(Enumerable.Repeat(true, prefab1ToAdd)
                .Concat(Enumerable.Repeat(false, prefab2ToAdd)).Shuffle(rng: rng)).ToList();
            
            var index = 0;

            var oldPointCount = points.Count;
            var shift = - oldSize / 2 + size / 2;
            var shiftedPoints = points.Select(x => x + shift).ToList();
            points = PoissonDiscSampler.RectangularPointSet(shiftedPoints,numberToPlace1 + numberToPlace2 - points.Count(), spawnSpace, minDistance,
                overflowMode: overflowMode, rng: rng, numSamplesBeforeRejection: maxAttemptsPerPoint).ToList();
            points = points.Select(x => x - shift).ToList();
            oldSize = size;
            foreach (var point in points.Skip(oldPointCount))
            {
                // Start here
                var pool = prefabAssignments[index++] ? prefab1Pool : prefab2Pool;
                var instance = pool.Add();
                
                var pos = point;

                if (hasLandscape) {
                    instance.localPosition = landscape.GetGroundAtLocalPoint(pos);
                    instance.GetOrAddComponent<LandscapeItem>();
                } else {
                    instance.localPosition = new Vector3(pos.x, 0, pos.y);
                }
                
                // Make sure the objects are visible in the editor.
                // A sequence can make them scale 0 when necessary, but we always want to see them when 
                // hitting the button.
                if (instance.localScale == Vector3.zero) {instance.localScale = Vector3.one;}

                if (points.IndexOf(point) < oldPointCount) continue;
                if (randomizeRotation)
                    instance.localRotation = Quaternion.Euler(0, rng.RangeInt(0, 360), 0);
            }

            if (prefab1Pool.activeChildCount < numberToPlace1)
                Debug.LogWarning("Only " + prefab1Pool.activeChildCount + " trees were placed out of an attempted " + numberToPlace1 + ".");
            if (prefab2Pool.activeChildCount < numberToPlace2)
                Debug.LogWarning("Only " + prefab2Pool.activeChildCount + " homes were placed out of an attempted " + numberToPlace2 + ".");
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
