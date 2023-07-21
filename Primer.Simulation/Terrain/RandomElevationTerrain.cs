using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Primer.Simulation
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class RandomElevationTerrain : MonoBehaviour
    {
        /// <summary>A height that is taller higher than "any" terrain can go.</summary>
        public const float HIGHER_THAN_POSSIBLE = 10000f;

        [OnValueChanged(nameof(Generate))]
        [Tooltip("The natural size of cuboid. Will be used to determine the number of vertices along each dimension as well.")]
        public Vector3Int size = new(50, 4, 50);

        #region Internal fields
        private MeshCollider meshColliderCache;
        private MeshCollider meshCollider => transform.GetOrAddComponent(ref meshColliderCache);

        private MeshFilter meshFilterCache;
        private MeshFilter meshFilter => transform.GetOrAddComponent(ref meshFilterCache);
        #endregion

        #region Noise settings
        [OnValueChanged(nameof(Generate))]
        public float noiseScale = 30;
        [OnValueChanged(nameof(Generate))]
        public int octaves = 5;
        [OnValueChanged(nameof(Generate))]
        [Range(0, 1)] public float persistance = 0.5f;
        [OnValueChanged(nameof(Generate))]
        public float lacunarity = 1;
        [OnValueChanged(nameof(Generate))]
        public int seed;

        [OnValueChanged(nameof(Generate))]
        [Tooltip("Moves the region we sample elevation values from.")]
        public Vector2 offset = new(51.11f, 0);
        #endregion

        #region Mesh settings
        [OnValueChanged(nameof(GenerateMesh))]
        [Tooltip("A constant that's multiplied with the unmultiplied elevation value (which will be bounded from 0 to 1).")]
        public float meshHeightMultiplier = 4;

        [OnValueChanged(nameof(GenerateMesh))]
        [Tooltip("Determines the roundingRadius of the edges. Max roundingRadius will be better if `size.y` is even.")]
        [Range(0, 1)]
        public float roundness = 1;

        [OnValueChanged(nameof(GenerateMesh))]
        [Tooltip("A constant that's subtracted from the elevation value.")]
        public float elevationOffset = 2;
        #endregion

        #region Mesh generation
        private float[,] noiseMap;

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
                new Vector2Int(size.x, size.z),
                noiseMap,
                meshHeightMultiplier,
                elevationOffset,
                0
            );

            meshCollider.sharedMesh = mesh;
            meshFilter.sharedMesh = mesh;
        }
        #endregion

        /// <summary>Gets the position of the ground at the given (x, z)</summary>
        /// <param name="x">Local-to-terrain x</param>
        /// <param name="z">Local-to-terrain y</param>
        /// <returns>The position of the ground in world space.</returns>
        public Vector3 GetGroundAt(float x, float z)
        {
            // Create a ray pointing straight down from high above the (x, z) coordinate given. The
            // ray needs to be in world coordinates.
            var pointAbove = transform.TransformPoint(new Vector3(x, HIGHER_THAN_POSSIBLE, z));
            var down = transform.TransformDirection(Vector3.down);
            var ray = new Ray(pointAbove, down);

            return meshCollider.Raycast(ray, out var hitInfo, float.PositiveInfinity)
                ? hitInfo.point
                : transform.TransformPoint(new Vector3(x, 0, z));
        }

#if UNITY_EDITOR
        [HideInInspector]
        public Texture2D dirtPathMask;

        public void PrepareTexture()
        {
            const TextureFormat desiredFormat = TextureFormat.R8;
            const int resolutionMultiplier = 1;

            if (dirtPathMask && dirtPathMask.format == desiredFormat) {
                if (dirtPathMask.width == size.x * resolutionMultiplier || dirtPathMask.height == size.z * resolutionMultiplier)
                    return;

                dirtPathMask = dirtPathMask.ResizeByBlit(size.x * resolutionMultiplier, size.z * resolutionMultiplier);
                Debug.Log("Resizing");
            }
            else {
                dirtPathMask = new Texture2D(size.x * resolutionMultiplier, size.z * resolutionMultiplier, desiredFormat, false);
                GetComponent<Renderer>().material.SetTexture("_PathMaskTex", dirtPathMask);
                Debug.Log("New one!");
            }
        }
#endif
    }
}
