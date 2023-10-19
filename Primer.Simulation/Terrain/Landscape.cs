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
            get => new(width, depth);
            set
            {
                width = value.x;
                depth = value.y;
            }
        }

        [SerializeField, HideInInspector]
        public int width = 50;
        [SerializeField, HideInInspector]
        public int depth = 50;
        
        [ShowInInspector]
        [HorizontalGroup("Size")]
        [LabelText("Width")]
        private int _width 
        { 
            get => width;
            set
            {
                width = value;
                Generate();
            }
        }

        [ShowInInspector]
        [HorizontalGroup("Size")]
        [LabelText("Depth")]
        private int _depth 
        { 
            get => depth;
            set
            {
                depth = value;
                Generate();
            }
        }
        #endregion

        #region Noise settings
        [SerializeField, HideInInspector]
        public float noiseScale = 10;

        [Title("Noise settings")]
        [ShowInInspector]
        private float _noiseScale {
            get => noiseScale;
            set {
                noiseScale = value;
                Generate();
            }
        }

        [SerializeField, HideInInspector]
        public int octaves = 5;

        [ShowInInspector]
        [PropertyRange(0, 10)]
        private int _octaves {
            get => octaves;
            set {
                octaves = value;
                Generate();
            }
        }

        [SerializeField, HideInInspector]
        public float persistance = 0.5f;

        [ShowInInspector]
        [PropertyRange(0, 1)]
        private float _persistance {
            get => persistance;
            set {
                persistance = value;
                Generate();
            }
        }

        [SerializeField, HideInInspector]
        public float lacunarity = 1;

        [ShowInInspector]
        private float _lacunarity {
            get => lacunarity;
            set {
                lacunarity = value;
                Generate();
            }
        }

        [SerializeField, HideInInspector]
        public int seed;

        [ShowInInspector]
        private int _seed {
            get => seed;
            set {
                seed = value;
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
        public float meshHeightMultiplier = 4;

        [Title("Mesh settings")]
        [ShowInInspector]
        [Tooltip("A constant that's multiplied with the unmultiplied elevation value (which will be bounded from 0 to 1).")]
        private float _meshHeightMultiplier {
            get => meshHeightMultiplier;
            set {
                meshHeightMultiplier = value;
                GenerateMesh();
            }
        }

        [FormerlySerializedAs("_roundness")] [SerializeField, HideInInspector]
        public float roundingRadius = 1;

        [ShowInInspector]
        [Tooltip("Determines the roundingRadius of the edges. Max roundingRadius will be better if `size.y` is even.")]
        [PropertyRange(0, 50)]
        private float _roundingRadius {
            get => roundingRadius;
            set {
                roundingRadius = Mathf.Min(value, Mathf.Min(size.x / 2, size.y / 2));
                GenerateMesh();
            }
        }

        [SerializeField, HideInInspector]
        public float elevationOffset = 0;

        [ShowInInspector]
        [Tooltip("A constant that's subtracted from the elevation value.")]
        [PropertyRange(0, 10)]
        private float _elevationOffset {
            get => elevationOffset;
            set {
                elevationOffset = value;
                GenerateMesh();
            }
        }

        [SerializeField, HideInInspector]
        public float edgeClampFactor;
        [ShowInInspector]
        [PropertyRange(0, 1)]
        private float _edgeClampFactor {
            get => edgeClampFactor;
            set {
                edgeClampFactor = value;
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
            meshRenderer.sharedMaterial ??= Resources.Load<Material>("grass");
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

            // This is a hack to ensure the collider updates properly so that the landscape items can find the ground
            meshCollider.enabled = false;
            meshCollider.enabled = true;
            // transform.parent.GetComponentsInChildren<LandscapeItem>().ForEach(item => item.DelayedTouchGround());
        }
        #endregion
        
        // public Vector3 GetGroundAt(Vector3 localPosition) => GetGroundAt(localPosition.x, localPosition.z);
        // public Vector3 GetGroundAt(Vector2 localPosition) => GetGroundAt(localPosition.x, localPosition.y);

        public Vector3 GetGroundAtLocalPoint(Vector3 worldPosition) => GetGroundAtLocalPoint(worldPosition.x, worldPosition.z);
        public Vector3 GetGroundAtLocalPoint(Vector2 worldPosition) => GetGroundAtLocalPoint(worldPosition.x, worldPosition.y);
        public Vector3 GetGroundAtWorldPoint(Vector3 worldPosition) => GetGroundAtWorldPoint(worldPosition.x, worldPosition.z);
        public Vector3 GetGroundAtWorldPoint(Vector2 worldPosition) => GetGroundAtWorldPoint(worldPosition.x, worldPosition.y);

        private Vector3 GetGroundAtLocalPoint(float x, float z)
        {
            var localPosition = transform.TransformPoint(new Vector3(x, 0, z));
            return transform.InverseTransformPoint(GetGroundAtWorldPoint(localPosition.x, localPosition.z));
        }

        /// <summary>Gets the position of the ground at the given (x, z)</summary>
        /// <param name="x">Local-to-terrain x</param>
        /// <param name="z">Local-to-terrain y</param>
        /// <returns>The position of the ground in world space.</returns>
        private Vector3 GetGroundAtWorldPoint(float x, float z)
        {
            // Create a ray pointing straight down from high above the (x, z) coordinate given.
            var pointAbove = new Vector3(x, size.y * 10, z);
            var down = transform.TransformDirection(Vector3.down);
            var ray = new Ray(pointAbove, down);

            var collision = meshCollider.Raycast(ray, out var hitInfo, float.PositiveInfinity);
            if (!collision) return new Vector3(x, 0, z);
            return hitInfo.point;
        }

        public MeshRenderer[] GetMeshRenderers() => root.GetComponentsInChildren<MeshRenderer>();
    }
}
