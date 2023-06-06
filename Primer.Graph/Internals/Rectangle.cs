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
    }
}
