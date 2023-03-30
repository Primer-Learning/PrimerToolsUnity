using System;
using System.Collections.Generic;
using UnityEngine;

// Copied and adapted from
// https://www.riccardostecca.net/articles/save_and_load_mesh_data_in_unity/

namespace Primer.Latex
{
    [Serializable]
    internal class SerializableMesh
    {
        [SerializeField] public float[] vertices;
        [SerializeField] public int[] triangles;
        [SerializeField] public float[] uv;
        [SerializeField] public float[] uv2;
        [SerializeField] public float[] normals;
        // [SerializeField] public Color[] colors;

        // Constructor: takes a mesh and fills out SerializableMeshInfo data structure which basically mirrors Mesh object's parts.
        public SerializableMesh(Mesh m)
        {
            // initialize vertices array.
            vertices = new float[m.vertexCount * 3];

            // Serialization: Vector3's values are stored sequentially.
            for (var i = 0; i < m.vertexCount; i++) {
                vertices[i * 3] = m.vertices[i].x;
                vertices[i * 3 + 1] = m.vertices[i].y;
                vertices[i * 3 + 2] = m.vertices[i].z;
            }

            // initialize triangles array
            triangles = new int[m.triangles.Length];

            // Mesh's triangles is an array that stores the indices, sequentially, of the vertices that form one face
            for (var i = 0;
                 i < m.triangles.Length;
                 i++) {
                triangles[i] = m.triangles[i];
            }

            // initialize uvs array
            uv = new float[m.uv.Length * 2];

            // uv's Vector2 values are serialized similarly to vertices' Vector3
            for (var i = 0;
                 i < m.uv.Length;
                 i++) {
                uv[i * 2] = m.uv[i].x;
                uv[i * 2 + 1] = m.uv[i].y;
            }

            // uv2
            uv2 = new float[m.uv2.Length];

            for (var i = 0; i < m.uv2.Length; i++) {
                uv[i * 2] = m.uv2[i].x;
                uv[i * 2 + 1] = m.uv2[i].y;
            }

            // normals are very important
            normals = new float[m.normals.Length * 3];

            // Serialization
            for (var i = 0; i < m.normals.Length; i++) {
                normals[i * 3] = m.normals[i].x;
                normals[i * 3 + 1] = m.normals[i].y;
                normals[i * 3 + 2] = m.normals[i].z;
            }

            // colors = new Color[m.colors.Length];
            //
            // for (var i = 0; i < m.colors.Length; i++) {
            //     colors[i] = m.colors[i];
            // }
        }

        // GetMesh gets a Mesh object from currently set data in this SerializableMeshInfo object.
        // Sequential values are deserialized to Mesh original data types like Vector3 for vertices.
        public Mesh GetMesh()
        {
            var m = new Mesh();
            var verticesList = new List<Vector3>();

            for (var i = 0; i < vertices.Length / 3; i++) {
                verticesList.Add(
                    new Vector3(
                        vertices[i * 3],
                        vertices[i * 3 + 1],
                        vertices[i * 3 + 2]
                    )
                );
            }

            m.SetVertices(verticesList);
            m.triangles = triangles;
            var uvList = new List<Vector2>();

            for (var i = 0; i < uv.Length / 2; i++) {
                uvList.Add(
                    new Vector2(
                        uv[i * 2],
                        uv[i * 2 + 1]
                    )
                );
            }

            m.SetUVs(0, uvList);
            var uv2List = new List<Vector2>();

            for (var i = 0; i < uv2.Length / 2; i++) {
                uv2List.Add(
                    new Vector2(
                        uv2[i * 2],
                        uv2[i * 2 + 1]
                    )
                );
            }

            m.SetUVs(1, uv2List);
            var normalsList = new List<Vector3>();

            for (var i = 0; i < normals.Length / 3; i++) {
                normalsList.Add(
                    new Vector3(
                        normals[i * 3],
                        normals[i * 3 + 1],
                        normals[i * 3 + 2]
                    )
                );
            }

            m.SetNormals(normalsList);
            // m.colors = colors;

            return m;
        }
    }
}
