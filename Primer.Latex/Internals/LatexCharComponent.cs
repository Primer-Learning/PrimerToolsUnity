using UnityEngine;

namespace Primer.Latex
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    internal class LatexCharComponent : MonoBehaviour
    {
        private MeshFilter meshFilterCache;
        private MeshFilter meshFilter => Meta.CachedGetComponent(ref meshFilterCache, transform);

        private MeshRenderer meshRendererCache;
        private MeshRenderer meshRenderer => Meta.CachedGetComponent(ref meshRendererCache, transform);

        public Vector3 position;
        public Rect bounds;

        public Mesh mesh {
            get => meshFilter.sharedMesh;
            set => meshFilter.sharedMesh = value;
        }

        public Material material {
            get => meshRenderer.material;
            set => meshRenderer.material = value;
        }

        public Color color {
            get => meshRenderer.GetColor();
            set => meshRenderer.SetColor(value);
        }

        public LatexChar ToLatexChar() => new(mesh, bounds, position);
    }
}
