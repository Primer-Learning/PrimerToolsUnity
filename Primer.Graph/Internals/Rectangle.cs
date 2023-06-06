using System;
using UnityEngine;

namespace Primer.Graph
{
    public enum RectPivot
    {
        BottomLeft,
        BottomCenter,
        Center,
    }

    [ExecuteAlways]
    public class Rectangle : MonoBehaviour
    {
        #region public Color color;
        private Color _color;
        public Color color {
            get => _color;
            set {
                _color = value;
                OnMaterialChange();
            }
        }
        #endregion

        #region public RectPivot pivot = RectPivot.Center;
        private RectPivot _pivot = RectPivot.Center;
        public RectPivot pivot {
            get => _pivot;
            set {
                _pivot = value;
                OnMeshChange();
            }
        }
        #endregion

        #region public float width;
        private float _width = 1;
        public float width {
            get => _width;
            set {
                _width = value;
                OnMeshChange();
            }
        }
        #endregion

        #region public float height;
        private float _height = 2;
        public float height {
            get => _height;
            set {
                _height = value;
                OnMeshChange();
            }
        }
        #endregion

        private Mesh mesh;
        private Material material;

        private void Update()
        {
            if (material is null)
                OnMaterialChange();

            if (mesh is null)
                OnMeshChange();

            Graphics.DrawMesh(mesh, transform.localToWorldMatrix, material, 0);
        }

        private void OnMaterialChange()
        {
            material ??= new Material(Shader.Find("Unlit/Color"));
            material.color = color;
        }

        private void OnMeshChange()
        {
            var vertices = pivot switch {
                RectPivot.BottomLeft => new [] {
                    new Vector3(0, 0),
                    new Vector3(width, 0),
                    new Vector3(0, height),
                    new Vector3(width, height),
                },
                RectPivot.BottomCenter => new [] {
                    new Vector3(-width / 2, 0),
                    new Vector3(width / 2, 0),
                    new Vector3(-width / 2, height),
                    new Vector3(width / 2, height),
                },
                RectPivot.Center =>  new [] {
                    new Vector3(-width / 2, -height / 2),
                    new Vector3(width / 2, -height / 2),
                    new Vector3(-width / 2, height / 2),
                    new Vector3(width / 2, height / 2),
                },
                _ => throw new ArgumentOutOfRangeException(),
            };

            if (mesh is null) {
                mesh = new Mesh {
                    vertices = vertices,
                    triangles = new [] {
                        0, 2, 1, // First triangle
                        2, 3, 1, // Second triangle
                    },
                };
            }
            else {
                mesh.vertices = vertices;
            }

            mesh.RecalculateNormals();
        }

        // GPT-4 suggestion for rounded corners
        // public int CornerVertices = 10; // number of vertices to use to draw the rounded corner
        // public float CornerRadius = 0.1f; // radius of the rounded corner

        // Vector3[] vertices = new Vector3[4 + CornerVertices];
        // int[] triangles = new int[6 + (CornerVertices - 1) * 3];
        //
        // // Rectangle vertices
        // vertices[0] = new Vector3(-Width / 2, -Height / 2);
        // vertices[1] = new Vector3(Width / 2 - CornerRadius, -Height / 2);
        // vertices[2] = new Vector3(-Width / 2, Height / 2);
        // vertices[3] = new Vector3(Width / 2 - CornerRadius, Height / 2);
        //
        // // Rectangle triangles
        // triangles[0] = 0; triangles[1] = 2; triangles[2] = 1;
        // triangles[3] = 2; triangles[4] = 3; triangles[5] = 1;
        //
        // // Top right corner vertices
        // for (int i = 0; i < CornerVertices; i++)
        // {
        //     float angle = Mathf.PI / 2 * i / (CornerVertices - 1);
        //     vertices[4 + i] = new Vector3(Width / 2 - CornerRadius + CornerRadius * Mathf.Cos(angle), Height / 2 - CornerRadius * Mathf.Sin(angle));
        // }
        //
        // // Top right corner triangles
        // for (int i = 0; i < CornerVertices - 1; i++)
        // {
        //     triangles[6 + i * 3] = 3;
        //     triangles[6 + i * 3 + 1] = 4 + i;
        //     triangles[6 + i * 3 + 2] = 4 + i + 1;
        // }
    }
}
