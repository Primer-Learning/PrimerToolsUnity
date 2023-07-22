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
            generator.AssignVertices();
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
        private Vector3[] normals;
        private float roundingRadius;
        private float edgeClampFactor;
        private Vector3[] vertices;
        private int[] mapFromUnusedToUsedSystematicIndices;
        private List<int> mapFromTempToRealVertices;
        private int xSize, ySize, zSize;

        private MeshGenerator() { }

        private void CreateVertices()
        {
            const int cornerVertices = 8;
            var edgeVertices = (xSize + ySize + zSize - 3) * 4;
            var faceVertices = ((xSize - 1) * (ySize - 1) + (xSize - 1) * (zSize - 1) +
                                (ySize - 1) * (zSize - 1)) * 2;

            vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
            mapFromUnusedToUsedSystematicIndices = new int[vertices.Length];
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
        }
        
        private void AssignVertices()
        {
            // Create a list of vertices where mapFromUnusedToUsedSystematicIndices[i] = i
            mapFromTempToRealVertices = new List<int>();
            var realVertices = new List<Vector3>();
            var realNormals = new List<Vector3>();
            for (var i = 0; i < mapFromUnusedToUsedSystematicIndices.Length; i++)
            {
                if (mapFromUnusedToUsedSystematicIndices[i] == i)
                {
                    realVertices.Add(vertices[i]);
                    realNormals.Add(normals[i]);
                    mapFromTempToRealVertices.Add(i);
                }
            }
            mesh.vertices = realVertices.ToArray();
            mesh.normals = realNormals.ToArray();
        }

        private void SetVertex(int i, int x, int y, int z)
        {
            vertices[i] = new Vector3(x, y, z);
            
            var innerDifference = CalculateDistanceFromInnerSurface(x, y, z, roundingRadius);
            
            var verticalFace = innerDifference.magnitude > roundingRadius;
            if (verticalFace) {
                normals[i] = innerDifference.normalized;
                vertices[i] = new Vector3(x, y, z) - innerDifference + innerDifference.normalized * roundingRadius;
                
                if (x == 0 || x == xSize || z == 0 || z == zSize) {
                    // This is an edge vertex, so it should map to itself
                    mapFromUnusedToUsedSystematicIndices[i] = i;
                }
                else if (x < xSize / 2)
                {
                    // Later, this will essentially tell the mesh to use the vertex to the right of this one
                    mapFromUnusedToUsedSystematicIndices[i] = i + 1;
                }
                else if (x > xSize / 2)
                {
                    // Later, this will essentially tell the mesh to use the vertex to the left of this one
                    mapFromUnusedToUsedSystematicIndices[i] = i - 1;
                }
                else
                {
                    // This is the middle vertex, so it should map to itself
                    mapFromUnusedToUsedSystematicIndices[i] = i;
                }
                
            }
            else
            {
                mapFromUnusedToUsedSystematicIndices[i] = i;
            }
            
            // Make bottom face normals point down
            // Also make the bottom edge normals point down at a 45 degree angle
            if (y == 0)
            {
                normals[i] = (Vector3.down + normals[i]).normalized; 
            }

            if (y == ySize)
            {
                normals[i] = (Vector3.up + normals[i]).normalized;
            }

            // If this point is in the bottom half, we're done.
            if (y < (float) ySize / 2)
                return;
            // The top half will be elevated by the height map
            // Top half instead of the very top because we want the triangles adjacent to the top and bottom
            // to be small so the normals and eventual textures look good

            var elevationAdjustment =
                BilinearSample(heightMap, vertices[i].x, vertices[i].z) * heightMultiplier;

            if (edgeClampFactor > 0) elevationAdjustment *= EdgeElevationDamping(vertices[i]);

            vertices[i].y += elevationAdjustment + elevationOffset;
        }
        
        private int GetUsedSystematicIndexFromSystematicIndex(int i)
        {
            if (mapFromUnusedToUsedSystematicIndices[i] == i) return i;
            return GetUsedSystematicIndexFromSystematicIndex(mapFromUnusedToUsedSystematicIndices[i]);
        }
        private int GetIndexOfRealVertexFromFakeIndex(int i)
        {
            // Debug.Log($"FakeVertexCount: {mapFromUnusedToUsedSystematicIndices.Length}, TempVertexCount: {mapFromTempToRealVertices.Count}, RealVertexCount: {mesh.vertices.Length}");
            return mapFromTempToRealVertices.IndexOf(GetUsedSystematicIndexFromSystematicIndex(i));
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
            var trianglesZ = new List<int>();
            var trianglesX =  new List<int>();
            var trianglesY = new List<int>();
            var ring = (xSize + zSize) * 2;
            var v = 0;

            for (var y = 0; y < ySize; y++, v++) {
                for (var q = 0; q < xSize; q++, v++)
                    SetQuad(trianglesZ, v, v + 1, v + ring, v + ring + 1);
                for (var q = 0; q < zSize; q++, v++)
                    SetQuad(trianglesX, v, v + 1, v + ring, v + ring + 1);
                for (var q = 0; q < xSize; q++, v++)
                    SetQuad(trianglesZ, v, v + 1, v + ring, v + ring + 1);
                for (var q = 0; q < zSize - 1; q++, v++)
                    SetQuad(trianglesX, v, v + 1, v + ring, v + ring + 1);
                SetQuad(trianglesX, v, v - ring + 1, v + ring, v + 1);
            }

            CreateTopFace(trianglesY, trianglesZ.Count + trianglesX.Count, ring);
            CreateBottomFace(trianglesY, trianglesZ.Count + trianglesX.Count + trianglesY.Count, ring);
            mesh.SetTriangles(trianglesZ.Concat(trianglesX).Concat(trianglesY).ToArray(), 0);
        }

        private int CreateTopFace(List<int> triangles, int t, int ring)
        {
            var v = ring * ySize;
            for (var x = 0; x < xSize - 1; x++, v++)
                SetQuad(triangles, v, v + 1, v + ring - 1, v + ring);
            SetQuad(triangles, v, v + 1, v + ring - 1, v + 2);

            var vMin = ring * (ySize + 1) - 1;
            var vMid = vMin + 1;
            var vMax = v + 2;

            for (var z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++) {
                SetQuad(triangles,  vMin, vMid, vMin - 1, vMid + xSize - 1);
                for (var x = 1; x < xSize - 1; x++, vMid++)
                    SetQuad(triangles, vMid, vMid + 1, vMid + xSize - 1, vMid + xSize);
                SetQuad(triangles, vMid, vMax, vMid + xSize - 1, vMax + 1);
            }

            var vTop = vMin - 2;
            SetQuad(triangles, vMin, vMid, vTop + 1, vTop);
            for (var x = 1; x < xSize - 1; x++, vTop--, vMid++)
                SetQuad(triangles, vMid, vMid + 1, vTop, vTop - 1);
            SetQuad(triangles, vMid, vTop - 2, vTop, vTop - 1);

            return t;
        }

        private int CreateBottomFace(List<int> triangles, int t, int ring)
        {
            var v = 1;
            var vMid = vertices.Length - (xSize - 1) * (zSize - 1);
            SetQuad(triangles, ring - 1, vMid, 0, 1);
            for (var x = 1; x < xSize - 1; x++, v++, vMid++)
                SetQuad(triangles, vMid, vMid + 1, v, v + 1);
            SetQuad(triangles, vMid, v + 2, v, v + 1);

            var vMin = ring - 2;
            vMid -= xSize - 2;
            var vMax = v + 2;

            for (var z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++) {
                SetQuad(triangles, vMin, vMid + xSize - 1, vMin + 1, vMid);
                for (var x = 1; x < xSize - 1; x++, vMid++)
                    SetQuad(triangles, vMid + xSize - 1, vMid + xSize, vMid, vMid + 1);
                SetQuad(triangles, vMid + xSize - 1, vMax + 1, vMid, vMax);
            }

            var vTop = vMin - 1;
            SetQuad(triangles, vTop + 1, vTop, vTop + 2, vMid);
            for (var x = 1; x < xSize - 1; x++, vTop--, vMid++)
                SetQuad(triangles, vTop, vTop - 1, vMid, vMid + 1);
            SetQuad(triangles, vTop, vTop - 1, vMid, vTop - 2);

            return t;
        }

        private void SetQuad(List<int> triangles, int v00, int v10, int v01, int v11)
        {
            SetTriangle(triangles, v00, v01, v10);
            SetTriangle(triangles, v01, v11, v10);
        }

        private void SetTriangle(List<int> triangles, int v0, int v1, int v2)
        {
            if (AreAllUnique(v0, v1, v2))
            {
                triangles.Add(GetIndexOfRealVertexFromFakeIndex(v0));
                triangles.Add(GetIndexOfRealVertexFromFakeIndex(v1));
                triangles.Add(GetIndexOfRealVertexFromFakeIndex(v2));
            }
        }

        private bool AreAllUnique(params int[] indices)
        {
            return indices.Distinct().Count() == indices.Length;
        }
    }
}
