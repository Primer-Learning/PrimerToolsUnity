using System.Drawing;
using Sirenix.OdinInspector;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Primer.Simulation
{
    [ExecuteAlways]
    public class Landscape : MonoBehaviour, IMeshController
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
        private Vector2 _offset = Vector2.zero;

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
        private float _meshHeightMultiplier = 4;

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
        private float _roundness = 1;

        [ShowInInspector]
        [Tooltip("Determines the roundness of the edges. Max roundness will be better if `size.y` is even.")]
        [PropertyRange(0, 50)]
        public float roundness {
            get => _roundness;
            set {
                _roundness = Mathf.Min(value, Mathf.Min(size.x / 2, size.z / 2));
                GenerateMesh();
            }
        }

        [SerializeField, HideInInspector]
        private float _elevationOffset = 2;

        [ShowInInspector]
        [Tooltip("A constant that's subtracted from the elevation value.")]
        public float elevationOffset {
            get => _elevationOffset;
            set {
                _elevationOffset = value;
                GenerateMesh();
            }
        }

        [ShowInInspector]
        public Material material {
            get => this.GetMaterial();
            set => this.SetMaterial(value);
        }

        [ShowInInspector]
        public Color color {
            get => this.GetColor();
            set => this.SetColor(value);
        }
        #endregion

        #region Internal fields
        private Transform rootCache;
        private Transform root => transform.FindOrCreate("Mesh");

        private MeshCollider meshColliderCache;
        private MeshCollider meshCollider => root.GetOrAddComponent(ref meshColliderCache);

        private MeshFilter meshFilterCache;
        private MeshFilter meshFilter => root.GetOrAddComponent(ref meshFilterCache);
        #endregion


        private void Awake()
        {
            var meshRenderer = root.GetOrAddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial ??= new Material(Shader.Find("Standard"));
            Generate();
        }


        #region Mesh generation

        private float[,] _noiseMap;
        public float[,] noiseMap {
            get
            {
                if (_noiseMap is null)
                {
                    _noiseMap = Noise.GenerateNoiseMap(
                        new Vector2Int(size.x + 1, size.z + 1),
                        seed,
                        noiseScale,
                        octaves,
                        persistance,
                        lacunarity,
                        offset
                    );
                };
                return _noiseMap;
            }
            set => _noiseMap = value;
        }

        [Title("Controls", HorizontalLine = false)]
        [Button]
        public void Generate()
        {
            noiseMap = Noise.GenerateNoiseMap(
                new Vector2Int(size.x + 1, size.z + 1),
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
                roundness,
                size,
                noiseMap,
                meshHeightMultiplier,
                elevationOffset
            );

            meshCollider.sharedMesh = mesh;
            meshFilter.sharedMesh = mesh;
            root.localPosition = ((Vector3)size) / -2f;
        }

        [Button]
        public void CleanUp()
        {
            var mesh = MeshGenerator.CreateMesh(
                roundness,
                size,
                noiseMap,
                meshHeightMultiplier,
                elevationOffset,
                cleanUp: true
            );

            meshCollider.sharedMesh = mesh;
            meshFilter.sharedMesh = mesh;
            root.localPosition = ((Vector3)size) / -2f;
        }
        #endregion


        // public Vector3 GetGroundAt(Vector3 localPosition) => GetGroundAt(localPosition.x, localPosition.z);
        // public Vector3 GetGroundAt(Vector2 localPosition) => GetGroundAt(localPosition.x, localPosition.y);

        public Vector3 GetGroundAtLocal(Vector3 localPosition) => GetGroundAtLocal(localPosition.x, localPosition.z);
        public Vector3 GetGroundAtLocal(Vector2 localPosition) => GetGroundAtLocal(localPosition.x, localPosition.y);
        public Vector3 GetGroundAt(Vector3 worldPosition) => GetGroundAt(worldPosition.x, worldPosition.z);
        public Vector3 GetGroundAt(Vector2 worldPosition) => GetGroundAt(worldPosition.x, worldPosition.y);

        private Vector3 GetGroundAtLocal(float x, float z)
        {
            // The ray needs to be in world coordinates.
            var worldPosition = transform.TransformPoint(new Vector3(x, 0, z));
            return GetGroundAt(worldPosition.x, worldPosition.z);
        }

        /// <summary>Gets the position of the ground at the given (x, z)</summary>
        /// <param name="x">Local-to-terrain x</param>
        /// <param name="z">Local-to-terrain y</param>
        /// <returns>The position of the ground in world space.</returns>
        private Vector3 GetGroundAt(float x, float z)
        {
            // Create a ray pointing straight down from high above the (x, z) coordinate given.
            var pointAbove = new Vector3(x, size.y * 2, z);
            var down = transform.TransformDirection(Vector3.down);
            var ray = new Ray(pointAbove, down);

            return meshCollider.Raycast(ray, out var hitInfo, float.PositiveInfinity)
                ? hitInfo.point
                : transform.TransformPoint(new Vector3(x, 0, z));
        }

        public MeshRenderer[] GetMeshRenderers() => root.GetComponentsInChildren<MeshRenderer>();
    }
}
