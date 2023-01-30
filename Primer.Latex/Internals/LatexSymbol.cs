using System;
using System.Linq;
using JetBrains.Annotations;
using Unity.VectorGraphics;
using UnityEngine;

namespace Primer.Latex
{
    [Serializable]
    public sealed class LatexSymbol
    {
        [NonSerialized] private readonly VectorUtils.Geometry geometry;
        [NonSerialized] private readonly Sprite sprite;
        [SerializeField] private Mesh meshCache;
        [SerializeReference] public readonly (Vector2 min, Vector2 max) bounds;

        [CanBeNull] public Mesh mesh => meshCache == null ? meshCache = CreateMesh(sprite) : meshCache;


        internal LatexSymbol(VectorUtils.Geometry geometry)
        {
            this.geometry = geometry;
            sprite = geometry.ConvertToSprite();

            var rect = VectorUtils.Bounds(geometry.TransformVertices());
            bounds = (rect.min, rect.max);
        }

        internal LatexSymbol(Mesh mesh, Rect rect)
        {
            meshCache = mesh;
            bounds = (rect.min, rect.max);
        }


        public override bool Equals(object other) =>
            other is LatexSymbol otherChar && geometry.IsSimilarEnough(otherChar.geometry);

        public override int GetHashCode() =>
            geometry.CalculateHashCode();


        public void Draw(Transform parent, Material material, Vector3 position, float scale)
        {
            var positionMod = Matrix4x4.Translate(position);
            var scaleMod = Matrix4x4.Scale(Vector3.one * scale);
            var matrix = parent.localToWorldMatrix * positionMod * scaleMod;
            Graphics.DrawMesh(mesh, matrix, material, 0);
        }

        private static Mesh CreateMesh(Sprite sprite)
        {
            if (sprite == null) return null;

            var normals = new Vector3(0f, 0f, -1f);
            var tangents = new Vector4(1f, 0f, 0f, -1f);

            return new Mesh {
                vertices = Array.ConvertAll(sprite.vertices, i => (Vector3)i),
                triangles = Array.ConvertAll(sprite.triangles, i => (int)i),
                normals = Enumerable.Repeat(normals, sprite.vertices.Length).ToArray(),
                tangents = Enumerable.Repeat(tangents, sprite.vertices.Length).ToArray(),
                uv = sprite.vertices,
            };
        }


#if UNITY_EDITOR
        public void DrawWireGizmos(Transform parent, Vector3 position, LatexGizmoMode features = LatexGizmoMode.Nothing)
        {
            // Invoking the property only once per call as this is called every update in the editor
            // ReSharper disable once LocalVariableHidesMember
            var mesh = this.mesh;

            if (features.HasFlag(LatexGizmoMode.WireFrame)) {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireMesh(
                    mesh,
                    parent.TransformPoint(position),
                    parent.rotation,
                    parent.lossyScale
                );
            }
            else {
                // This is useful to do because the editor will select our game object if they
                // click on this gizmo (despite being transparent).
                Gizmos.color = Color.clear;
                Gizmos.DrawMesh(
                    mesh,
                    parent.TransformPoint(position),
                    parent.rotation,
                    parent.lossyScale
                );
            }

            if (!features.HasFlag(LatexGizmoMode.Normals) &&
                !features.HasFlag(LatexGizmoMode.Tangents)) return;

            var vertices = mesh.vertices;
            var normals = mesh.normals;
            var tangents = mesh.tangents;

            for (var i = 0; i < vertices.Length; ++i) {
                var vertexPosition = parent.TransformPoint(position + vertices[i]);

                if (features.HasFlag(LatexGizmoMode.Normals)) {
                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(vertexPosition, parent.TransformDirection(normals[i]) * 0.2f);
                }

                if (features.HasFlag(LatexGizmoMode.Tangents)) {
                    Gizmos.color = Color.black;
                    Gizmos.DrawRay(vertexPosition, parent.TransformDirection(tangents[i]) * 0.2f);
                }
            }
        }
#endif
    }
}
