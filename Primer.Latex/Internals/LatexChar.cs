using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.VectorGraphics;
using UnityEngine;

namespace Primer.Latex
{
    [Serializable]
    public sealed record LatexChar
    {
        [SerializeField] public readonly VectorUtils.Geometry geometry;
        [SerializeField] public readonly Vector3 position;
        [NonSerialized] public readonly Sprite sprite;

        [NonSerialized] private Mesh meshCache;
        [CanBeNull] public Mesh mesh => meshCache ??= CreateMesh(sprite);


        /// <summary>
        ///     This is expected to happen when a LatexRenderer is
        ///     (a) initialized from defaults,
        ///     (b) set via a preset in the editor
        ///     (c) and occasionally when undoing/redoing.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The sprite assets are added as subassets of the scene the LatexRenderer is in, and can be
        ///         garbage collected any time if there's no LatexRenderer referencing them. This means when
        ///         redoing/undoing you can arrive in a state where the LatexRenderer is pointing at sprites
        ///         that have been cleaned up. Fortunately Unity notices when deserializing and the sprites
        ///         appear as null values in our list.
        ///     </para>
        ///     <para>
        ///         Presets that refer to a Sprite also don't prevent the sprite from being garbage collected
        ///         (and the preset could be applied to a LatexRenderer in a different scene anyways). So
        ///         presets often cause this same issue. Finally, when editing a preset directly, we actually
        ///         set its stored _sprites value to null directly since we need to make sure we never have a
        ///         mismatch between Latex & Headers text and the stored sprites.
        ///     </para>
        ///     <para>
        ///         I suspect there's a way to handle these situations using hooks into various parts of the
        ///         editor. But a decently thorough dive into the options had me arrive at the conclusion that
        ///         the approach here (just recognizing the mismatch and having the inspector rebuild) is the
        ///         simplest approach. Hopefully as my domain knowledge of the editor improves I'll think of an
        ///         even cleaner way though.
        ///     </para>
        /// </remarks>
        public bool isSpriteValid => (bool)sprite;


        #region Equality (comparison to others)
        public bool IsSamePosition(LatexChar other) => other != null && position == other.position;

        // TODO: this must be tested, how do we know if two geometries are identical?
        public bool IsSameGeometry(LatexChar other) => other != null && Equals(geometry, other.geometry);

        public bool Equals(LatexChar other) =>
            IsSamePosition(other) && IsSameGeometry(other);

        public override int GetHashCode() => HashCode.Combine(geometry, position);
        #endregion


        public LatexChar(VectorUtils.Geometry geometry, Vector2 offset)
        {
            this.geometry = geometry;
            position = CalculatePosition(geometry, offset);
            // TODO: try moving sprite generation to Draw()
            sprite = ConvertGeometryToSprite(geometry);
        }


        #region Convert from SVG
        public const float SVG_PIXELS_PER_UNIT = 10f;
        private static Sprite ConvertGeometryToSprite(VectorUtils.Geometry geometry)
        {
            var geometries = new List<VectorUtils.Geometry> {geometry};
            return VectorUtils.BuildSprite(
                geometries,
                SVG_PIXELS_PER_UNIT,
                VectorUtils.Alignment.Center,
                Vector2.zero,
                128,
                true
            );
        }

        private static Vector2 CalculatePosition(VectorUtils.Geometry geometry, Vector2 offset)
        {
            var position = VectorUtils.Bounds(
                from vertex in geometry.Vertices
                select geometry.WorldTransform * vertex / SVG_PIXELS_PER_UNIT
            ).center;

            position -= offset;

            // This matches the way flipYAxis would work in BuildSprite if we gave it all of the geometry in the SVG
            // rather than just one at a time.
            position.y = -position.y;
            return position;
        }
        #endregion


        #region Draw with raw Unity Graphics
        public void Draw(Transform parent, Material material)
        {
            var matrix = parent.localToWorldMatrix * Matrix4x4.Translate(position);
            Graphics.DrawMesh(mesh, matrix, material, 0);
        }

        private static Mesh CreateMesh(Sprite sprite)
        {
            if (sprite is null) return null;

            return new Mesh {
                vertices = Array.ConvertAll(sprite.vertices, i => (Vector3)i),
                triangles = Array.ConvertAll(sprite.triangles, i => (int)i),
                normals =
                    Enumerable.Repeat(new Vector3(0f, 0f, -1f), sprite.vertices.Length)
                        .ToArray(),
                tangents =
                    Enumerable.Repeat(new Vector4(1f, 0f, 0f, -1f), sprite.vertices.Length)
                        .ToArray(),
                uv = sprite.vertices
            };
        }
        #endregion


#if UNITY_EDITOR
        [Flags]
        public enum GizmoMode
        {
            Nothing = 0b0,
            WireFrame = 0b1,
            Normals = 0b10,
            Tangents = 0b100,
            Everything = 0b111
        }

        public void DrawWireGizmos(Transform parent, GizmoMode features = GizmoMode.Nothing)
        {
            // Invoking the property only once per call as this is called every update in the editor
            // ReSharper disable once LocalVariableHidesMember
            var mesh = this.mesh;

            if (features.HasFlag(GizmoMode.WireFrame)) {
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

            if (!features.HasFlag(GizmoMode.Normals) &&
                !features.HasFlag(GizmoMode.Tangents)) return;

            var vertices = mesh.vertices;
            var normals = mesh.normals;
            var tangents = mesh.tangents;

            for (var i = 0; i < vertices.Length; ++i) {
                var vertexPosition = parent.TransformPoint(position + vertices[i]);

                if (features.HasFlag(GizmoMode.Normals)) {
                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(vertexPosition, parent.TransformDirection(normals[i]) * 0.2f);
                }

                if (features.HasFlag(GizmoMode.Tangents)) {
                    Gizmos.color = Color.black;
                    Gizmos.DrawRay(vertexPosition, parent.TransformDirection(tangents[i]) * 0.2f);
                }
            }
        }
#endif
    }
}
