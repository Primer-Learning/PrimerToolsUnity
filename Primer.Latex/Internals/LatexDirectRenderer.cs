using System;
using System.Linq;
using UnityEngine;

namespace Primer.Latex
{
    internal class LatexDirectRenderer
    {
        internal record DrawSpec(Mesh Mesh, Material Material, Vector3 Position);

        internal DrawSpec[] drawSpecs = {};

        private LatexChar[] lastCharacters;
        private Material lastMaterial;


        public void SetCharacters(LatexChar[] chars, Material material)
        {
            if (chars == lastCharacters && material == lastMaterial) return;

            drawSpecs = new DrawSpec[chars.Length];

            for (var i = 0; i < chars.Length; i++) {
                drawSpecs[i] = new DrawSpec(CreateMesh(chars[i].sprite), material, chars[i].position);
            }

            lastCharacters = chars;
            lastMaterial = material;
        }

        public void Draw(Transform parent)
        {
            for (var i = 0; i < drawSpecs.Length; i++) {
                var drawSpec = drawSpecs[i];

                Graphics.DrawMesh(
                    drawSpec.Mesh,
                    parent.localToWorldMatrix * Matrix4x4.Translate(drawSpec.Position),
                    drawSpec.Material,
                    0
                );
            }
        }

        private static Mesh CreateMesh(Sprite sprite)
        {
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
            Nothing = 0b0,
            WireFrame = 0b1,
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
