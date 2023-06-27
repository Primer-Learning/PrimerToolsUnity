using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Primer.Simulation
{
    [ExecuteAlways]
    public class Landscape : MonoBehaviour
    {
        #region public Vector3Int size;
        [SerializeField, HideInInspector]
        private Vector3Int _size = new(50, 4, 50);

        [ShowInInspector]
        [Tooltip("The natural size of cuboid. Will be used to determine the number of vertices along each dimension as well.")]
        public Vector3Int size {
            get => _size;
            set {
                if (value.y < 4) {
                    Debug.Log($"{nameof(Landscape)}'s height must be at least 4");
                    value.y = 4;
                }

                if (value.y > value.x || value.y > value.z) {
                    Debug.Log($"{nameof(Landscape)}'s height can't be greater than its width and depth");
                    value.x = value.y;
                    value.z = value.y;
                }

                _size = value;
                Generate();
            }
        }
        #endregion

        #region Noise settings
        [SerializeField, HideInInspector]
        private float _noiseScale = 10;

        [Title("Noise settings")]
        [ShowInInspector]
        public float noiseScale {
            get => _noiseScale;
            set {
                _noiseScale = value;
                Generate();
            }
        }

        [SerializeField, HideInInspector]
        private int _octaves = 5;

        [ShowInInspector]
        [PropertyRange(0, 10)]
        public int octaves {
            get => _octaves;
            set {
                _octaves = value;
                Generate();
            }
        }

        [SerializeField, HideInInspector]
        private float _persistance = 0.5f;

        [ShowInInspector]
        [PropertyRange(0, 1)]
        public float persistance {
            get => _persistance;
            set {
                _persistance = value;
                Generate();
            }
        }

        [SerializeField, HideInInspector]
        private float _lacunarity = 1;

        [ShowInInspector]
        public float lacunarity {
            get => _lacunarity;
            set {
                _lacunarity = value;
                Generate();
            }
        }

        [SerializeField, HideInInspector]
        private int _seed;

        [ShowInInspector]
        public int seed {
            get => _seed;
            set {
                _seed = value;
                Generate();
            }
        }

        [SerializeField, HideInInspector]
        public Vector2 _offset = Vector2.zero;

        [ShowInInspector]
        [Tooltip("Moves the region we sample elevation values from.")]
        public Vector2 offset {
            get => _offset;
            set {
                _offset = value;
                Generate();
            }
        }
        #endregion

        #region Mesh settings
        [SerializeField, HideInInspector]
        public float _meshHeightMultiplier = 4;

        [Title("Mesh settings")]
        [ShowInInspector]
        [Tooltip("A constant that's multiplied with the unmultiplied elevation value (which will be bounded from 0 to 1).")]
        public float meshHeightMultiplier {
            get => _meshHeightMultiplier;
            set {
                _meshHeightMultiplier = value;
                GenerateMesh();
            }
        }

        [SerializeField, HideInInspector]
        public float _roundness = 1;

        [ShowInInspector]
        [Tooltip("Determines the roundness of the edges. Max roundness will be better if `size.y` is even.")]
        [PropertyRange(0, 1)]
        public float roundness {
            get => _roundness;
            set {
                _roundness = value;
                GenerateMesh();
            }
        }

        [SerializeField, HideInInspector]
        public float _elevationOffset = 2;

        [ShowInInspector]
        [Tooltip("A constant that's subtracted from the elevation value.")]
        public float elevationOffset {
            get => _elevationOffset;
            set {
                _elevationOffset = value;
                GenerateMesh();
            }
        }
        #endregion

        #region Internal fields
        private Transform rootCache;
        private Transform root => transform.FindOrCreate("Landscape mesh");

        private MeshCollider meshColliderCache;
        private MeshCollider meshCollider => root.GetOrAddComponent(ref meshColliderCache);

        private MeshFilter meshFilterCache;
        private MeshFilter meshFilter => root.GetOrAddComponent(ref meshFilterCache);
        #endregion


        private void Awake()
        {
            var meshRenderer = root.GetOrAddComponent<MeshRenderer>();
            meshRenderer.material ??= new Material(Shader.Find("Standard"));
            Generate();
        }


        #region Mesh generation
        private float[,] noiseMap;

        [Title("Controls")]
        [Button]
        public void Generate()
        {
            noiseMap = Noise.GenerateNoiseMap(
                size.To2D(),
                seed,
                noiseScale,
                octaves,
                persistance,
                lacunarity,
                offset
            );

            GenerateMesh();
        }

        private void GenerateMesh()
        {
            var mesh = MeshGenerator.CreateMesh(
                Mathf.FloorToInt(roundness * size.y / 2),
                size,
                noiseMap,
                meshHeightMultiplier,
                elevationOffset
            );

            transform.FindOrCreate("Landscape mesh");


            meshCollider.sharedMesh = mesh;
            meshFilter.sharedMesh = mesh;
        }
        #endregion


        public Vector3 GetGroundAt(Vector2 localPosition) => GetGroundAt(localPosition.x, localPosition.y);

        /// <summary>Gets the position of the ground at the given (x, z)</summary>
        /// <param name="x">Local-to-terrain x</param>
        /// <param name="z">Local-to-terrain y</param>
        /// <returns>The position of the ground in world space.</returns>
        public Vector3 GetGroundAt(float x, float z)
        {
            // Create a ray pointing straight down from high above the (x, z) coordinate given. The
            // ray needs to be in world coordinates.
            var pointAbove = transform.TransformPoint(new Vector3(x, size.y * 2, z));
            var down = transform.TransformDirection(Vector3.down);
            var ray = new Ray(pointAbove, down);

            return meshCollider.Raycast(ray, out var hitInfo, float.PositiveInfinity)
                ? hitInfo.point
                : transform.TransformPoint(new Vector3(x, 0, z));
        }
    }
}
