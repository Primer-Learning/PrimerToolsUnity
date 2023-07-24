using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace Primer.Simulation
{
    internal class MeshGenerator
    {
        public static Mesh CreateMesh(float roundness, Vector2Int size, float[,] heightMap,
            float heightMultiplier, float elevationOffset, float edgeClampDistance, bool cleanUp = false)
        {
            // This code was initially copied from
            // https://catlikecoding.com/unity/tutorials/rounded-cube/ and it was designed there
            // to be in a MonoBehaviour, thus it's fairly stateful. This class could be refactored
            // to be static if we wanted to.
            
            var generator = new MeshGenerator {
                roundingRadius = roundness,
                xSize = size.x,
                ySize = 3,
                zSize = size.y,
                mesh = new Mesh(),
                heightMultiplier = heightMultiplier,
                heightMap = heightMap,
                elevationOffset = elevationOffset,
                edgeClampFactor = edgeClampDistance
            };

            generator.CreateVertices();
            generator.CreateTriangles();

            if (cleanUp)
            {
                generator.CleanDuplicateVerticesAndZeroAreaTriangles();
            }

            generator.mesh.RecalculateNormals();
            generator.mesh.RecalculateTangents();

            return generator.mesh;
        }

        // Implementation below

        private float elevationOffset;
        private float[,] heightMap;
        private float heightMultiplier;

        private Mesh mesh;
        private float roundingRadius;
        private float edgeClampFactor;
        
        private List<Vector3> vertices;
        // These two contain the same information
        // But including the second prevents slow calls to IndexOf when setting triangles
        private List<Vector3Int> vertexIndexToXYZ;
        private int[,,] vertexXYZToIndex;
        
        private int xSize, ySize, zSize;

        private MeshGenerator() { }

        private void CreateVertices()
        {
            vertices = new List<Vector3>();
            vertexIndexToXYZ = new List<Vector3Int>();
            vertexXYZToIndex = new int[xSize + 1, ySize + 1, zSize + 1];
            // uv = new Vector2[vertices.Length];
            // normals = new Vector3[vertices.Length];

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

            mesh.vertices = vertices.ToArray();
        }

        private void SetVertex(int i, int x, int y, int z)
        {
            var candidate = new Vector3(x, y, z);
            
            var innerDifference = CalculateDistanceFromInnerSurface(x, y, z, roundingRadius);

            var verticalFace = innerDifference.magnitude > roundingRadius;
            if (verticalFace) {
                // normals[i] = innerDifference.normalized;
                candidate = new Vector3(x, y, z) - innerDifference + innerDifference.normalized * roundingRadius;
            }
            
            // If this point is in the bottom half, we're done.
            if (y < (float)ySize / 2)
            {
                vertices.Add(candidate);
                vertexIndexToXYZ.Add(new Vector3Int(x, y, z));
                vertexXYZToIndex[x, y, z] = i;
                return;
            }
            // The top half will be elevated by the height map
            // Top half instead of the very top because we want the triangles adjacent to the top and bottom
            // to be small so the normals and eventual textures look good

            var elevationAdjustment =
                BilinearSample(heightMap, candidate.x, candidate.z) * heightMultiplier;

            if (edgeClampFactor > 0) elevationAdjustment *= EdgeElevationDamping(candidate);

            candidate.y += elevationAdjustment + elevationOffset;
            
            vertices.Add(candidate);
            vertexIndexToXYZ.Add(new Vector3Int(x, y, z));
            vertexXYZToIndex[x, y, z] = i;
        }

        private float EdgeElevationDamping(Vector3 vertex)
        {
            if (roundingRadius == 0) return 1;
            
            // Recalculate this because the position has changed since it was previously calculated
            var innerDifference = CalculateDistanceFromInnerSurface(vertex.x, vertex.y, vertex.z, roundingRadius);
            
            // Ranges from 0 at the edge to 1
            var normalizedOuterDistance = 1 - innerDifference.magnitude / roundingRadius;
            
            return Mathf.Clamp(normalizedOuterDistance / edgeClampFactor, 0, 1);
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

        // The outer mesh is rounded by creating a virtual inner surface,
        // then moving vertices to a point within the "roundingRadius" distance of the inner surface
        private Vector3 CalculateDistanceFromInnerSurface(float x, float y, float z, float radius)
        {
            float buffer = 0.0001f;
            var radiusWithBuffer = radius + buffer;
            var position = new Vector3(x, y, z);
            if (x < radiusWithBuffer)
            {
                x = radiusWithBuffer;
            }
            else if (x > xSize - radiusWithBuffer)
            {
                x = xSize - radiusWithBuffer;
            }
            if (z < radiusWithBuffer)
            {
                z = radiusWithBuffer;
            }
            else if (z > zSize - radiusWithBuffer)
            {
                z = zSize - radiusWithBuffer;
            }
            return position - new Vector3(x, y, z);
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
            var triangles = new int[xSize * ySize * 12 + ySize * zSize * 12 + xSize * zSize * 12];
            int t = 0;

            for (var y = 0; y < ySize; y++) {
                for (var x = 0; x < xSize; x++)
                    t = SetQuad(triangles, t, new Vector3Int(x, y, 0), new Vector3Int(x + 1, y, 0), new Vector3Int(x, y + 1, 0), new Vector3Int(x + 1, y + 1, 0));
                for (var z = 0; z < zSize; z++)
                    t = SetQuad(triangles, t, new Vector3Int(0, y, z), new Vector3Int(0, y + 1, z), new Vector3Int(0, y, z + 1), new Vector3Int(0, y + 1, z + 1));
                for (var x = 0; x < xSize; x++)
                    t = SetQuad(triangles, t, new Vector3Int(x, y, zSize), new Vector3Int(x, y + 1, zSize), new Vector3Int(x + 1, y, zSize), new Vector3Int(x + 1, y + 1, zSize));
                for (var z = 0; z < zSize; z++)
                    t = SetQuad(triangles, t, new Vector3Int(xSize, y, z), new Vector3Int(xSize, y, z + 1), new Vector3Int(xSize, y + 1, z), new Vector3Int(xSize, y + 1, z + 1));
            }

            t = CreateTopFace(triangles, t);
            t = CreateBottomFace(triangles, t);
            mesh.SetTriangles(triangles, 0);
        }

        private int CreateTopFace(int[] triangles, int t)
        {
            for (var z = 0; z < zSize; z++)
            for (var x = 0; x < xSize; x++)
                t = SetQuad(triangles, t, new Vector3Int(x, ySize, z), new Vector3Int(x + 1, ySize, z), new Vector3Int(x, ySize, z + 1), new Vector3Int(x + 1, ySize, z + 1));

            return t;
        }

        private int CreateBottomFace(int[] triangles, int t)
        {
            for (var z = 0; z < zSize; z++)
            for (var x = 0; x < xSize; x++)
                t = SetQuad(triangles, t, new Vector3Int(x, 0, z), new Vector3Int(x, 0, z + 1), new Vector3Int(x + 1, 0, z), new Vector3Int(x + 1, 0, z + 1));

            return t;
        }

        private int SetQuad(int[] triangles, int i, Vector3Int v00, Vector3Int v10, Vector3Int v01, Vector3Int v11)
        {
            triangles[i] = vertexXYZToIndex[v00.x, v00.y, v00.z];
            triangles[i + 1] = triangles[i + 4] = vertexXYZToIndex[v01.x, v01.y, v01.z];
            triangles[i + 2] = triangles[i + 3] = vertexXYZToIndex[v10.x, v10.y, v10.z];
            triangles[i + 5] = vertexXYZToIndex[v11.x, v11.y, v11.z];
            return i + 6;
        }

        private bool AreAllUnique(params int[] indices)
        {
            return indices.Distinct().Count() == indices.Length;
        }
    }
}
