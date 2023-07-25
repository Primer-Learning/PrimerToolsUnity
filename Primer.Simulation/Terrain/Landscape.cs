using System.Drawing;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Color = UnityEngine.Color;

namespace Primer.Simulation
{
    [ExecuteAlways]
    public class Landscape : MonoBehaviour, IMeshController
    {
        #region public Vector2Int size;
        public Vector2Int size
        {
            get => new(_width, _depth);
            set
            {
                _width = value.x;
                _depth = value.y;
                Generate();
            }
        }

        private int _width = 50;
        private int _depth = 50;
        
        [ShowInInspector]
        [HorizontalGroup("Size")]
        [LabelText("Width")]
        public int width 
        { 
            get => _width;
            set
            {
                _width = value;
                Generate();
            }
        }

        [ShowInInspector]
        [HorizontalGroup("Size")]
        [LabelText("Depth")]
        public int depth 
        { 
            get => _depth;
            set
            {
                _depth = value;
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

        [FormerlySerializedAs("_roundness")] [SerializeField, HideInInspector]
        private float _roundingRadius = 1;

        [ShowInInspector]
        [Tooltip("Determines the roundingRadius of the edges. Max roundingRadius will be better if `size.y` is even.")]
        [PropertyRange(0, 50)]
        public float roundingRadius {
            get => _roundingRadius;
            set {
                _roundingRadius = Mathf.Min(value, Mathf.Min(size.x / 2, size.y / 2));
                GenerateMesh();
            }
        }

        [SerializeField, HideInInspector]
        private float _elevationOffset = 0;

        [ShowInInspector]
        [Tooltip("A constant that's subtracted from the elevation value.")]
        [PropertyRange(0, 10)]
        public float elevationOffset {
            get => _elevationOffset;
            set {
                _elevationOffset = value;
                GenerateMesh();
            }
        }

        [FormerlySerializedAs("_edgeClampDistance")] [SerializeField, HideInInspector]
        private float _edgeClampFactor;
        [ShowInInspector]
        [PropertyRange(0, 1)]
        public float edgeClampFactor {
            get => _edgeClampFactor;
            set {
                _edgeClampFactor = value;
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
        
        [SerializeField, HideInInspector]
        private bool _showNormals;
        
        [ShowInInspector]
        public bool showNormals
        {
            get => _showNormals;
            set
            {
                _showNormals = value;
                var normalViewer = root.GetComponent<MeshNormalViewer>();
                if (normalViewer is not null)
                {
                    if (value) normalViewer.DrawNormals();
                    else normalViewer.ClearNormals();
                }
            }
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
                        new Vector2Int(size.x + 1, size.y + 1),
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
                new Vector2Int(size.x + 1, size.y + 1),
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
                roundingRadius,
                size,
                noiseMap,
                meshHeightMultiplier,
                elevationOffset,
                edgeClampFactor
            );

            meshCollider.sharedMesh = mesh;
            meshFilter.sharedMesh = mesh;
            root.localPosition = new Vector3(size.x, 0, size.y) / -2f;

            if (_showNormals)
            {
                var normalViewer = root.GetComponent<MeshNormalViewer>();
                if (normalViewer is not null && normalViewer.enabled)
                {
                    normalViewer.DrawNormals();
                }
            }
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
