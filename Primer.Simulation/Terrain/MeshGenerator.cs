using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Simulation
{
    internal class MeshGenerator
    {
        public static Mesh CreateMesh(float roundness, Vector3Int size, float[,] heightMap,
            float heightMultiplier, float elevationOffset, bool cleanUp = false)
        {
            // This code was initially copied from
            // https://catlikecoding.com/unity/tutorials/rounded-cube/ and it was designed there
            // to be in a MonoBehaviour, thus it's fairly stateful. This class could be refactored
            // to be static if we wanted to.
            
            var generator = new MeshGenerator {
                roundness = roundness,
                xSize = size.x,
                ySize = size.y,
                zSize = size.z,
                mesh = new Mesh(),
                heightMultiplier = heightMultiplier,
                heightMap = heightMap,
                elevationOffset = elevationOffset,
            };

            generator.CreateVertices();
            generator.CreateTriangles();

            if (cleanUp)
            {
                generator.CleanDuplicateVerticesAndZeroAreaTriangles();
            }

            // generator.mesh.RecalculateNormals();
            generator.mesh.RecalculateTangents();

            return generator.mesh;
        }


        // Implementation below

        private float elevationOffset;
        private float[,] heightMap;
        private float heightMultiplier;

        private Mesh mesh;
        private Vector3[] normals;
        private float roundness;
        private Vector2[] uv;
        private Vector3[] vertices;
        private int xSize, ySize, zSize;

        private MeshGenerator() { }

        private void CreateVertices()
        {
            const int cornerVertices = 8;
            var edgeVertices = (xSize + ySize + zSize - 3) * 4;
            var faceVertices = ((xSize - 1) * (ySize - 1) + (xSize - 1) * (zSize - 1) +
                                (ySize - 1) * (zSize - 1)) * 2;

            vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
            uv = new Vector2[vertices.Length];
            normals = new Vector3[vertices.Length];

            var v = 0;
            for (var y = 0; y <= ySize; y++) {
                for (var x = 0; x <= xSize; x++) SetVertex(v++, x, y, 0);
                for (var z = 1; z <= zSize; z++) SetVertex(v++, xSize, y, z);
                for (var x = xSize - 1; x >= 0; x--) SetVertex(v++, x, y, zSize);
                for (var z = zSize - 1; z > 0; z--) SetVertex(v++, 0, y, z);
            }

            // This is the top face
            for (var z = 1; z < zSize; z++)
            for (var x = 1; x < xSize; x++)
                SetVertex(v++, x, ySize, z);

            for (var z = 1; z < zSize; z++)
            for (var x = 1; x < xSize; x++)
                SetVertex(v++, x, 0, z);

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uv;
        }

        private void SetVertex(int i, int x, int y, int z)
        {
            var inner = vertices[i] = new Vector3(x, y, z);
            
            // There is probably a more compact way to do this.
            // Each outer if statement checks if the vertex is within the roundness distance from one of the corners
            // in both directions.
            // Then the inner if statement checks pythagorean distance from the "inner" corner.
            if (x < roundness && z < roundness)
            {
                if ((roundness - x)  * (roundness - x) + (roundness - z) * (roundness - z) > roundness * roundness)
                {
                    inner.x = roundness;
                    inner.z = roundness;
                }
            }
            else if (x < roundness && z > zSize - roundness)
            {
                if ((roundness - x)  * (roundness - x) + (zSize - roundness - z) * (zSize - roundness - z) > roundness * roundness)
                {
                    inner.x = roundness;
                    inner.z = zSize - roundness;
                }

            }
            else if (x > xSize - roundness && z < roundness)
            {
                if ((xSize - roundness - x)  * (xSize - roundness - x) + (roundness - z) * (roundness - z) > roundness * roundness)
                {
                    inner.x = xSize - roundness;
                    inner.z = roundness;
                }
            }
            else if (x > xSize - roundness && z > zSize - roundness)
            {
                if ((xSize - roundness - x)  * (xSize - roundness - x) + (zSize - roundness - z) * (zSize - roundness - z) > roundness * roundness)
                {
                    inner.x = xSize - roundness;
                    inner.z = zSize - roundness;
                }
            }


            normals[i] = (vertices[i] - inner).normalized;
            vertices[i] = inner + normals[i] * roundness;
            uv[i] = new Vector2(x / (float)xSize, z / (float)zSize);

            // If we're on the top face, we want to elevate the vertices based on the height map
            if (y < ySize)
                return;

            var elevationAdjustment =
                BilinearSample(heightMap, vertices[i].x, vertices[i].z) * heightMultiplier +
                elevationOffset;

            // We don't want the terrain to dip below ground level once it starts to approach
            // the edge of the map, so we'll smoothly raise it up.
            if (elevationAdjustment < 0) {
                // Distance from current vertex on the positive-y face to an edge, in vertices
                var distanceToEdge = Mathf.Min(1 + x, xSize - x - 1, 1 + z, zSize - z - 1);
            
                // Maximum possible value of distanceToEdge
                var maxDistance = Math.Max((xSize + 1) / 2, (ySize + 1) / 2);
            
                var edgeDecay = Mathf.Clamp(Mathf.Pow(distanceToEdge * 4f / maxDistance, 2f), 0f, 1f);
                elevationAdjustment *= edgeDecay;
            }

            vertices[i].y += elevationAdjustment;
        }

        private void CleanDuplicateVerticesAndZeroAreaTriangles()
        {
            float tolerance = 0.1f; // Adjust this value as needed.
            var comparer = new Vector3Comparer(tolerance);
            Dictionary<Vector3, int> distinctVertices = new Dictionary<Vector3, int>(comparer);

            // Create a list of unique vertices and record the mapping from old to new indices
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                Vector3 vertex = mesh.vertices[i];

                distinctVertices.TryAdd(vertex, distinctVertices.Count);
            }
    
            // Update the triangles to use the new indices
            // Remove triangles with zero area
            List<int> newTriangles = new List<int>();
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                var indices = new[]
                {
                    distinctVertices[mesh.vertices[mesh.triangles[i]]],
                    distinctVertices[mesh.vertices[mesh.triangles[i + 1]]],
                    distinctVertices[mesh.vertices[mesh.triangles[i + 2]]]
                };

                if (indices.Distinct().Count() == indices.Length)
                {
                    newTriangles.AddRange(indices);
                }
            }

            Debug.Log("Old vertex count: " + mesh.vertexCount);
            Debug.Log("New vertex count: " + distinctVertices.Count);
            
            Debug.Log("Old max triangle index: " + mesh.triangles.Max());
            Debug.Log("New max triangle index: " + newTriangles.Max());
            
            Debug.Log("Removed " + (mesh.vertices.Length - distinctVertices.Count) + " vertices");
            Debug.Log("Removed " + (mesh.triangles.Length - newTriangles.Count) / 3 + " triangles");
            
            mesh.triangles = newTriangles.ToArray();
            mesh.vertices = distinctVertices.Keys.ToArray();
        }

        /// <summary>
        /// Samples a 2D array, interpolating between adjacent values if given fractional
        /// coordinates.
        /// </summary>
        private static float BilinearSample(float[,] array, float x, float y)
        {
            var integerX = Mathf.FloorToInt(x);
            var fractionX = x - integerX;

            var integerY = Mathf.FloorToInt(y);
            var fractionY = y - integerY;

            int ClampDimension(int index, int dimension)
            {
                // Debug.Log($"Clamping {index} to {array.GetLength(dimension)}");
                return Math.Min(array.GetLength(dimension) - 1, index);
            }

            // Modified from https://stackoverflow.com/a/22153181/3920202.
            // Find the four corners we're interpolating between.
            var bottomLeft = array[integerX, integerY];
            var bottomRight = array[ClampDimension(integerX + 1, 0), integerY];
            var topLeft  = array[integerX, ClampDimension(integerY + 1, 1)];
            var topRight = array[ClampDimension(integerX + 1, 0), ClampDimension(integerY + 1, 1)];
            
            // Interpolate horizontally
            var topInterpolation = fractionX * (topRight - topLeft) + topLeft;
            var bottomInterpolation = fractionX * (bottomRight - bottomLeft) + bottomLeft;
            
            // Interpolate vertically
            return fractionY * (topInterpolation - bottomInterpolation) + bottomInterpolation;
        }

        private void CreateTriangles()
        {
            var trianglesZ = new int[xSize * ySize * 12];
            var trianglesX = new int[ySize * zSize * 12];
            var trianglesY = new int[xSize * zSize * 12];
            var ring = (xSize + zSize) * 2;
            int tZ = 0, tX = 0, tY = 0, v = 0;

            for (var y = 0; y < ySize; y++, v++) {
                for (var q = 0; q < xSize; q++, v++)
                    tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
                for (var q = 0; q < zSize; q++, v++)
                    tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
                for (var q = 0; q < xSize; q++, v++)
                    tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
                for (var q = 0; q < zSize - 1; q++, v++)
                    tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
                tX = SetQuad(trianglesX, tX, v, v - ring + 1, v + ring, v + 1);
            }

            tY = CreateTopFace(trianglesY, tY, ring);
            tY = CreateBottomFace(trianglesY, tY, ring);
            mesh.SetTriangles(trianglesZ.Concat(trianglesX).Concat(trianglesY).ToArray(), 0);
        }

        private int CreateTopFace(int[] triangles, int t, int ring)
        {
            var v = ring * ySize;
            for (var x = 0; x < xSize - 1; x++, v++)
                t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
            t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

            var vMin = ring * (ySize + 1) - 1;
            var vMid = vMin + 1;
            var vMax = v + 2;

            for (var z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++) {
                t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + xSize - 1);
                for (var x = 1; x < xSize - 1; x++, vMid++)
                    t = SetQuad(triangles, t, vMid, vMid + 1, vMid + xSize - 1, vMid + xSize);
                t = SetQuad(triangles, t, vMid, vMax, vMid + xSize - 1, vMax + 1);
            }

            var vTop = vMin - 2;
            t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
            for (var x = 1; x < xSize - 1; x++, vTop--, vMid++)
                t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
            t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);

            return t;
        }

        private int CreateBottomFace(int[] triangles, int t, int ring)
        {
            var v = 1;
            var vMid = vertices.Length - (xSize - 1) * (zSize - 1);
            t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
            for (var x = 1; x < xSize - 1; x++, v++, vMid++)
                t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
            t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

            var vMin = ring - 2;
            vMid -= xSize - 2;
            var vMax = v + 2;

            for (var z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++) {
                t = SetQuad(triangles, t, vMin, vMid + xSize - 1, vMin + 1, vMid);
                for (var x = 1; x < xSize - 1; x++, vMid++)
                    t = SetQuad(triangles, t, vMid + xSize - 1, vMid + xSize, vMid, vMid + 1);
                t = SetQuad(triangles, t, vMid + xSize - 1, vMax + 1, vMid, vMax);
            }

            var vTop = vMin - 1;
            t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
            for (var x = 1; x < xSize - 1; x++, vTop--, vMid++)
                t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

            return t;
        }

        private static int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)
        {
            triangles[i] = v00;
            triangles[i + 1] = triangles[i + 4] = v01;
            triangles[i + 2] = triangles[i + 3] = v10;
            triangles[i + 5] = v11;
            return i + 6;
        }

        private bool AreAllUnique(params int[] indices)
        {
            return indices.Distinct().Count() == indices.Length;
        }
    }
}
