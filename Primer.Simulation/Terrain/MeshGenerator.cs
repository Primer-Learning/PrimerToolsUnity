using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using Sirenix.Utilities;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
        private int[,,] vertexXYZToIndex;
        
        private List<int> triangles;
        
        private int xSize, ySize, zSize;

        private MeshGenerator() { }

        private void CreateVertices()
        {
            vertices = new List<Vector3>();
            vertexXYZToIndex = new int[xSize + 1, ySize + 1, zSize + 1];

            for (var y = 0; y <= ySize; y++) {
                for (var x = 0; x <= xSize; x++) SetVertex(x, y, 0);
                for (var z = 1; z <= zSize; z++) SetVertex(xSize, y, z);
                for (var x = xSize - 1; x >= 0; x--) SetVertex(x, y, zSize);
                for (var z = zSize - 1; z > 0; z--) SetVertex(0, y, z);
            }

            // This is the top face
            for (var z = 1; z < zSize; z++)
            for (var x = 1; x < xSize; x++)
                SetVertex(x, ySize, z);

            for (var z = 1; z < zSize; z++)
            for (var x = 1; x < xSize; x++)
                SetVertex(x, 0, z);

            mesh.vertices = vertices.ToArray();
        }

        private void SetVertex(int x, int y, int z)
        {
            var candidate = new Vector3(x, y, z);
            
            var innerDifference = CalculateDistanceFromInnerSurface(x, y, z, roundingRadius);
            if (innerDifference.magnitude > 0 && roundingRadius > 0)
            {
                var factorThatExtendsToNonroundedEdge =
                    roundingRadius / Mathf.Max(Mathf.Abs(innerDifference.x), Mathf.Abs(innerDifference.z));
                var fullLength = innerDifference * factorThatExtendsToNonroundedEdge;
                
                var innerEdge = new Vector3(x, y, z) - innerDifference;
                var addition = innerDifference * roundingRadius / fullLength.magnitude;
                
                candidate = innerEdge + addition;
            }

            // If this point is in the bottom half, we're done.
            if (y < (float)ySize / 2)
            {
                vertices.Add(candidate);
                vertexXYZToIndex[x, y, z] = vertices.Count - 1;
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
            vertexXYZToIndex[x, y, z] = vertices.Count - 1;
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

        // The outer mesh is rounded by creating a virtual inner surface,
        // then moving vertices to a point within the "roundingRadius" distance of the inner surface
        private Vector3 CalculateDistanceFromInnerSurface(float x, float y, float z, float radius)
        {
            float buffer = 0.001f;
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
        private float BilinearSample(float[,] array, float x, float y)
        {
            x = Mathf.Clamp(x, 0, xSize);
            y = Mathf.Clamp(y, 0, zSize);
            
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
            triangles = new List<int>();

            for (var y = 0; y < ySize; y++) {
                for (var x = 0; x < xSize; x++)
                    SetQuad(new Vector3Int(x, y, 0), new Vector3Int(x + 1, y, 0), new Vector3Int(x, y + 1, 0), new Vector3Int(x + 1, y + 1, 0));
                for (var z = 0; z < zSize; z++)
                    SetQuad(new Vector3Int(0, y, z), new Vector3Int(0, y + 1, z), new Vector3Int(0, y, z + 1), new Vector3Int(0, y + 1, z + 1));
                for (var x = 0; x < xSize; x++)
                    SetQuad(new Vector3Int(x, y, zSize), new Vector3Int(x, y + 1, zSize), new Vector3Int(x + 1, y, zSize), new Vector3Int(x + 1, y + 1, zSize));
                for (var z = 0; z < zSize; z++)
                    SetQuad(new Vector3Int(xSize, y, z), new Vector3Int(xSize, y, z + 1), new Vector3Int(xSize, y + 1, z), new Vector3Int(xSize, y + 1, z + 1));
            }

            triangles.AddRange(CreateTopFace());
            triangles.AddRange(CreateBottomFace());
            // Debug.Log(triangles.Max());
            // Debug.Log(vertices.Count);
            mesh.SetTriangles(triangles.ToArray(), 0);
        }

        private List<int> CreateTopFace()
        {
            var tris = new List<int>();
            for (var z = 0; z < zSize; z++)
            for (var x = 0; x < xSize; x++)
                if (x < xSize / 2 ^ z < zSize / 2)
                    SetQuad(new Vector3Int(x, ySize, z), new Vector3Int(x + 1, ySize, z), new Vector3Int(x, ySize, z + 1), new Vector3Int(x + 1, ySize, z + 1));
                else
                    SetQuad(new Vector3Int(x, ySize, z + 1), new Vector3Int(x, ySize, z), new Vector3Int(x + 1, ySize, z + 1), new Vector3Int(x + 1, ySize, z));

            return tris;
        }

        private List<int> CreateBottomFace()
        {
            var tris = new List<int>();
            for (var z = 0; z < zSize; z++)
            for (var x = 0; x < xSize; x++)
                if (x < xSize / 2 ^ z < zSize / 2)
                    SetQuad(new Vector3Int(x, 0, z), new Vector3Int(x, 0, z + 1), new Vector3Int(x + 1, 0, z), new Vector3Int(x + 1, 0, z + 1));
                else
                    SetQuad(new Vector3Int(x + 1, 0, z), new Vector3Int(x, 0, z), new Vector3Int(x + 1, 0, z + 1), new Vector3Int(x, 0, z + 1));

                    return tris;
        }

        private void SetQuad(Vector3Int v00, Vector3Int v10, Vector3Int v01, Vector3Int v11)
        {
            // Get the indices
            var i00 = vertexXYZToIndex[v00.x, v00.y, v00.z];
            var i10 = vertexXYZToIndex[v10.x, v10.y, v10.z];
            var i01 = vertexXYZToIndex[v01.x, v01.y, v01.z];
            var i11 = vertexXYZToIndex[v11.x, v11.y, v11.z];
            
            SetTriangle(i00, i01, i10);
            SetTriangle(i10, i01, i11);
        }

        private void  SetTriangle(int i0, int i1, int i2)
        {
            if (AreAllUnique(i0, i1, i2))
                triangles.AddRange(new[] { i0, i1, i2 });
            // else Debug.Log("Skipping triangle because of duplicate vertices");
        }

        private bool AreAllUnique(params int[] indices)
        {
            return new HashSet<int>(indices).Count == indices.Length;
        }
    }
}
