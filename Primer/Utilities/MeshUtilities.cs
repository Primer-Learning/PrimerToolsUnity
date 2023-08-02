using UnityEngine;

namespace Primer
{
    public static class MeshUtilities
    {
        public static void MakeDoubleSided(ref Vector3[] vertices, ref int[] triangles)
        {
            // Prepare arrays to contain vertices and triangles for both sides
            // If doubleSided is false, the empty entries won't matter
            var verticesDouble = new Vector3[vertices.Length * 2];
            var trianglesDouble = new int[triangles.Length * 2];
                
            // Fill the vertices and triangles for the first side
            vertices.CopyTo(verticesDouble, 0);
            triangles.CopyTo(trianglesDouble, 0);
                
            // Add the copied set of vertices
            vertices.CopyTo(verticesDouble, vertices.Length);
            // Add the copied set of triangles, but reverse the order of the vertices for each one
            // so that the normals point in the opposite direction.
            for (var i = 0; i < triangles.Length; i += 3)
            {
                trianglesDouble[i + triangles.Length] = triangles[i + 2] + vertices.Length;
                trianglesDouble[i + 1 + triangles.Length] = triangles[i + 1] + vertices.Length;
                trianglesDouble[i + 2 + triangles.Length] = triangles[i] + vertices.Length;
            }

            vertices = verticesDouble;
            triangles = trianglesDouble;
        }
    }
}