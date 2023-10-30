using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Color = UnityEngine.Color;

namespace Primer.Simulation
{
    [ExecuteAlways]
    public class Landscape : MonoBehaviour, IMeshController
    {
        private bool meshUpToDate;
        
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
        private int _width = 50;
        public int width
        {
            get => _width;
            set
            {
                if (_width != value) meshUpToDate = false;
                _width = value;
            }
        }
        [ShowInInspector]
        [HorizontalGroup("Size")]
        [LabelText("Width")]
        private int InspectorWidth 
        { 
            get => width;
            set
            {
                width = value;
                Generate();
            }
        }

        [SerializeField, HideInInspector]
        private int _depth = 50;
        public int depth
        {
            get => _depth;
            set
            {
                if (_depth != value) meshUpToDate = false;
                _depth = value;
            }
        }
        [ShowInInspector]
        [HorizontalGroup("Size")]
        [LabelText("Depth")]
        private int InspectorDepth 
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
        private float _noiseScale = 10;
        public float noiseScale
        {
            get => _noiseScale;
            set
            {
                if (_noiseScale != value) meshUpToDate = false;
                _noiseScale = value;
            }
        }
        [Title("Noise settings")]
        [ShowInInspector]
        private float InspectorNoiseScale {
            get => noiseScale;
            set {
                noiseScale = value;
                Generate();
            }
        }

        [SerializeField, HideInInspector]
        private int _octaves = 5;
        public int octaves
        {
            get => _octaves;
            set
            {
                if (_octaves != value) meshUpToDate = false;
                _octaves = value;
            }
        }

        [ShowInInspector]
        [PropertyRange(0, 10)]
        private int InspectorOctaves {
            get => octaves;
            set {
                octaves = value;
                Generate();
            }
        }

        [FormerlySerializedAs("persistance")] [SerializeField, HideInInspector]
        private float _persistence = 0.5f;
        public float persistence
        {
            get => _persistence;
            set
            {
                if (_persistence != value) meshUpToDate = false;
                _persistence = value;
            }
        }

        [ShowInInspector]
        [PropertyRange(0, 1)]
        private float InspectorPersistance {
            get => persistence;
            set {
                persistence = value;
                Generate();
            }
        }

        [SerializeField, HideInInspector]
        private float _lacunarity = 1;
        public float lacunarity
        {
            get => _lacunarity;
            set
            {
                if (_lacunarity != value) meshUpToDate = false;
                _lacunarity = value;
            }
        }

        [ShowInInspector]
        private float InspectorLacunarity {
            get => lacunarity;
            set {
                lacunarity = value;
                Generate();
            }
        }

        [SerializeField, HideInInspector]
        private int _seed;
        public int seed
        {
            get => _seed;
            set
            {
                if (_seed != value) meshUpToDate = false;
                _seed = value;
            }
        }

        [ShowInInspector]
        private int InspectorSeed {
            get => seed;
            set {
                seed = value;
                Generate();
            }
        }

        [SerializeField, HideInInspector]
        private Vector2 _offset;
        public Vector2 offset
        {
            get => _offset;
            set
            {
                if (_offset != value) meshUpToDate = false;
                _offset = value;
            }
        }

        [ShowInInspector]
        [Tooltip("Moves the region we sample elevation values from.")]
        public Vector2 InspectorOffset {
            get => offset;
            set {
                offset = value;
                Generate();
            }
        }
        #endregion

        #region Mesh settings
        [SerializeField, HideInInspector]
        private float _meshHeightMultiplier = 4;
        public float meshHeightMultiplier
        {
            get => _meshHeightMultiplier;
            set
            {
                if (_meshHeightMultiplier != value) meshUpToDate = false;
                _meshHeightMultiplier = value;
            }
        }

        [Title("Mesh settings")]
        [ShowInInspector]
        [Tooltip("A constant that's multiplied with the unmultiplied elevation value (which will be bounded from 0 to 1).")]
        private float InspectorMeshHeightMultiplier {
            get => meshHeightMultiplier;
            set {
                meshHeightMultiplier = value;
                GenerateMesh();
            }
        }

        [SerializeField, HideInInspector]
        private float _roundingRadius = 1;
        public float roundingRadius
        {
            get => _roundingRadius;
            set
            {
                if (_roundingRadius != value) meshUpToDate = false;
                _roundingRadius = value;
            }
        }

        [ShowInInspector]
        [Tooltip("Determines the roundingRadius of the edges. Max roundingRadius will be better if `size.y` is even.")]
        [PropertyRange(0, 200)]
        private float InspectorRoundingRadius {
            get => roundingRadius;
            set {
                roundingRadius = Mathf.Min(value, Mathf.Min((float) size.x / 2, (float) size.y / 2));
                GenerateMesh();
            }
        }

        [SerializeField, HideInInspector]
        private float _elevationOffset;
        public float elevationOffset
        {
            get => _elevationOffset;
            set
            {
                if (_elevationOffset != value) meshUpToDate = false;
                _elevationOffset = value;
            }
        }

        [ShowInInspector]
        [Tooltip("A constant that's subtracted from the elevation value.")]
        [PropertyRange(0, 10)]
        private float InspectorElevationOffset {
            get => elevationOffset;
            set {
                elevationOffset = value;
                GenerateMesh();
            }
        }

        [SerializeField, HideInInspector] 
        private float _edgeClampFactor = 0;
        public float edgeClampFactor
        {
            get => _edgeClampFactor;
            set
            {
                if (_edgeClampFactor != value) meshUpToDate = false;
                _edgeClampFactor = value;
            }
        }
        [ShowInInspector]
        [PropertyRange(0, 1)]
        private float InspectorEdgeClampFactor {
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

        // private float[,] _noiseMap;
        public float[,] noiseMap {
            get
            {
                return Noise.GenerateNoiseMap(
                    new Vector2Int(size.x + 1, size.y + 1),
                    seed,
                    noiseScale,
                    octaves,
                    persistence,
                    lacunarity,
                    offset
                );
            }
            // set => _noiseMap = value;
        }

        [Title("Controls", HorizontalLine = false)]
        [Button]
        public void Generate()
        {
            if (meshUpToDate) return;
            
            // noiseMap = Noise.GenerateNoiseMap(
            //     new Vector2Int(size.x + 1, size.y + 1),
            //     seed,
            //     noiseScale,
            //     octaves,
            //     persistence,
            //     lacunarity,
            //     offset
            // );

            GenerateMesh();
        }

        private void GenerateMesh()
        {
            if (!meshUpToDate)
            {
                var mesh = MeshGenerator.CreateMesh(
                    roundingRadius,
                    size,
                    noiseMap,
                    meshHeightMultiplier,
                    elevationOffset,
                    edgeClampFactor
                );
                meshUpToDate = true;
                
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

            // This is a hack to ensure the collider updates properly so that the landscape items can find the ground
            // meshCollider.enabled = false;
            // meshCollider.enabled = true;
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
