using System;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Unity.VectorGraphics;
using UnityEngine;

namespace Primer.Latex
{
    [Serializable]
    public sealed class LatexSymbol
    {
        public readonly VectorUtils.Geometry geometry;
        [NonSerialized] private readonly Sprite sprite;
        [NonSerialized] private Mesh meshCache;


        internal LatexSymbol(VectorUtils.Geometry geometry)
        {
            this.geometry = geometry;
            sprite = geometry.ConvertToSprite();
        }

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


        public override bool Equals(object other)
            => other is LatexSymbol otherChar && geometry.IsSimilarEnough(otherChar.geometry);

        public override int GetHashCode()
            => throw new NotImplementedException();


        public void Draw(Transform parent, Material material, Vector3 position, float scale)
        {
            var positionMod = Matrix4x4.Translate(position);
            var scaleMod = Matrix4x4.Scale(Vector3.one * scale);
            var matrix = parent.localToWorldMatrix * positionMod * scaleMod;
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
                uv = sprite.vertices,
            };
        }


#if UNITY_EDITOR

        // for debug proposes
        public (string code, int index) source;

        public string Source {
            get {
                var (code, index) = source;
                var keyword = new Regex(@"\\\w+");

                var latex = keyword.Replace(code, "/")
                                   .Replace("$", "")
                                   .Replace("{", "(")
                                   .Replace("}", ")")
                                   .Replace(" ", "");

                var start = latex[..index];
                var middle = index < latex.Length ? latex[index] : ' ';
                var end = latex[(index + 1)..];

                return $"{start} >>> {middle} <<< {end} ({index})";
            }
        }


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

            if (!features.HasFlag(LatexGizmoMode.Normals) && !features.HasFlag(LatexGizmoMode.Tangents)) return;

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
