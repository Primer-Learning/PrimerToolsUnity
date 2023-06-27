using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Primer.Simulation
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class RandomElevationTerrain : MonoBehaviour
    {
        /// <summary>A height that is taller higher than "any" terrain can go.</summary>
        public const float HIGHER_THAN_POSSIBLE = 10000f;

        public float noiseScale = 30;

        public int octaves = 5;
        [Range(0, 1)] public float persistance = 0.5f;
        public float lacunarity = 1;

        public int seed;

        [Tooltip("Moves the region we sample elevation values from.")]
        public Vector2 offset = new(51.11f, 0);

        [Tooltip("A constant that's multiplied with the unmultiplied elevation value (which will be bounded from 0 to 1).")]
        public float meshHeightMultiplier = 4;

        [Tooltip("If true, the mesh will be regenerated in the editor when a field is changed.")]
        public bool autoUpdate = true;

        [Tooltip("Determines the roundness of the edges. Max roundness will be better if `size.y` is even.")]
        [Range(0, 1)]
        public float roundness = 1;

        [Tooltip("A constant that's subtracted from the elevation value.")]
        public float elevationOffset = 2;

        [Tooltip("The natural size of cuboid. Will be used to determine the number of vertices along each dimension as well.")]
        public Vector3Int size = new(50, 4, 50);

        [HideInInspector]
        public Texture2D dirtPathMask;

        public void PrepareTexture()
        {
            const TextureFormat desiredFormat = TextureFormat.R8;
            const int resolutionMultiplier = 1;

            if (dirtPathMask && dirtPathMask.format == desiredFormat) {
                if (dirtPathMask.width != size.x * resolutionMultiplier && dirtPathMask.height != size.z * resolutionMultiplier) {
                    dirtPathMask = dirtPathMask.ResizeByBlit(size.x * resolutionMultiplier,
                    size.z * resolutionMultiplier);
                    Debug.Log("Resizing");
                }
            }
            else {
                dirtPathMask = new Texture2D(size.x * resolutionMultiplier, size.z * resolutionMultiplier, desiredFormat, false);
                GetComponent<Renderer>().material.SetTexture("_PathMaskTex", dirtPathMask);
                Debug.Log("New one!");
            }
        }

#if UNITY_EDITOR
        private SerializedObject lastValue;

        private static bool CompareSerializedObjects(SerializedObject a, SerializedObject b)
        {
            // This will recursively check every property, even though it seems like it ought to
            // only check the first. I found this out through experiment.
            return SerializedProperty.DataEquals(a.GetIterator(), b.GetIterator());
        }

        public void Update()
        {
            if (Application.isPlaying) return;
            var current = new SerializedObject(this);

            if (autoUpdate && lastValue is not null && !CompareSerializedObjects(current, lastValue)) {
                GenerateMap();
            }

            lastValue = current;
        }
        #endif

        public void GenerateMap()
        {
            var noiseMap = Noise.GenerateNoiseMap(size.x, size.z, seed, noiseScale, octaves,
                persistance, lacunarity, offset);

            GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().sharedMesh =
                MeshGenerator.CreateMesh(Mathf.FloorToInt(roundness * size.y / 2), size, noiseMap,
                    meshHeightMultiplier, elevationOffset);
        }

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

            return GetComponent<MeshCollider>().Raycast(ray, out var hitInfo, float.PositiveInfinity)
                ? hitInfo.point
                : transform.TransformPoint(new Vector3(x, 0, z));
        }
    }
}
