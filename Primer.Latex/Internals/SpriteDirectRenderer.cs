using System;
using System.Linq;
using UnityEngine;

namespace Primer.Latex
{
    internal class SpriteDirectRenderer
    {
        internal record DrawSpec(Mesh Mesh, Material Material, Vector3 Position);

        internal DrawSpec[] drawSpecs = {};

        /// <summary>The last values given to SetSprites.</summary>
        (Sprite[] sprites, Vector3[] spritePositions, Material material) lastSeenSprites;

        public void SetSprites(Sprite[] sprites, Vector3[] spritePositions, Material material) {
            var args = (sprites, spritePositions, material);
            if (args == lastSeenSprites) return;

            drawSpecs = sprites.Zip(
                spritePositions,
                (sprite, position) => new DrawSpec(CreateMesh(sprite), material, position)
            ).ToArray();

            lastSeenSprites = args;
        }

        public void Draw(Transform parent) {
            foreach (var drawSpec in drawSpecs) {
                Graphics.DrawMesh(
                    drawSpec.Mesh,
                    parent.localToWorldMatrix * Matrix4x4.Translate(drawSpec.Position),
                    drawSpec.Material,
                    0
                );
            }
        }

        static Mesh CreateMesh(Sprite sprite) {
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

#if UNITY_EDITOR
        [Flags]
        public enum GizmoMode
        {
            Nothing = 0,
            WireFrame = 1,
            Normals = 0b10,
            Tangents = 0b100,
            Everything = 0b111
        }

        public void DrawWireGizmos(Transform parent, GizmoMode features = GizmoMode.Nothing) {
            foreach (var drawSpec in drawSpecs) {
                if (features.HasFlag(GizmoMode.WireFrame)) {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireMesh(
                        drawSpec.Mesh,
                        parent.TransformPoint(drawSpec.Position),
                        parent.rotation,
                        parent.lossyScale
                    );
                }
                else {
                    // This is useful to do because the editor will select our game object if they
                    // click on this gizmo (despite being transparent).
                    Gizmos.color = Color.clear;
                    Gizmos.DrawMesh(
                        drawSpec.Mesh,
                        parent.TransformPoint(drawSpec.Position),
                        parent.rotation,
                        parent.lossyScale
                    );
                }

                if (!features.HasFlag(GizmoMode.Normals) &&
                    !features.HasFlag(GizmoMode.Tangents)) continue;

                var vertices = drawSpec.Mesh.vertices;
                var normals = drawSpec.Mesh.normals;
                var tangents = drawSpec.Mesh.tangents;

                for (var i = 0; i < vertices.Length; ++i) {
                    var position = parent.TransformPoint(drawSpec.Position + vertices[i]);

                    if (features.HasFlag(GizmoMode.Normals)) {
                        Gizmos.color = Color.green;
                        Gizmos.DrawRay(position, parent.TransformDirection(normals[i]) * 0.2f);
                    }

                    if (features.HasFlag(GizmoMode.Tangents)) {
                        Gizmos.color = Color.black;
                        Gizmos.DrawRay(position, parent.TransformDirection(tangents[i]) * 0.2f);
                    }
                }
            }
        }
#endif
    }
}
