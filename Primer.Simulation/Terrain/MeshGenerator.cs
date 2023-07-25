using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Primer.Simulation
{
    internal class MeshGenerator
    {
        public static Mesh CreateMesh(float roundingRadius, Vector2Int size, float[,] heightMap,
            float heightMultiplier, float elevationOffset, float edgeClampFactor)
        {
            // This code was initially copied from
            // https://catlikecoding.com/unity/tutorials/rounded-cube/ and it was designed there
            // to be in a MonoBehaviour, thus it's fairly stateful. This class could be refactored
            // to be static if we wanted to.
            
            var generator = new MeshGenerator {
                roundingRadius = roundingRadius,
                xSize = size.x,
                ySize = 3,
                zSize = size.y,
                mesh = new Mesh(),
                heightMultiplier = heightMultiplier,
                heightMap = heightMap,
                elevationOffset = elevationOffset,
                edgeClampFactor = edgeClampFactor
            };

            generator.CreateVertices();
            generator.CreateTriangles();

            generator.mesh.RecalculateNormals();
            generator.CorrectBottomEdgeNormals();
            if (edgeClampFactor > 0 && roundingRadius > 1)
            {
                generator.CorrectTopEdgeNormals();
            }
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

        #region Vertices
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
            
            var innerDifference = CalculateDifferenceVectorFromInnerSurface(x, y, z, roundingRadius);
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

        // The outer mesh is rounded by creating a virtual inner surface,
        // then moving vertices to a point within the "roundingRadius" distance of the inner surface
        private Vector3 CalculateDifferenceVectorFromInnerSurface(float x, float y, float z, float radius)
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
        #endregion

        #region Elevation methods
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

        private float EdgeElevationDamping(Vector3 vertex)
        {
            if (roundingRadius == 0) return 1;
            
            // Recalculate this because the position has changed since it was previously calculated
            var innerDistance = CalculateDifferenceVectorFromInnerSurface(vertex.x, vertex.y, vertex.z, roundingRadius);
            var outerDistance = roundingRadius - innerDistance.magnitude;
            var maxDistance = roundingRadius * edgeClampFactor;
            if (outerDistance > maxDistance) return 1;
            
            return Mathf.Sin(outerDistance / maxDistance * Mathf.PI / 2);
        }
        #endregion
        
        #region Triangles
        private void CreateTriangles()
        {
            triangles = new List<int>();

            for (var y = 0; y < ySize; y++)
            {
                for (var x = 0; x < xSize; x++)
                {
                    var rotate90 = x < xSize / 2 ^ y < ySize / 2;
                    SetQuad(new Vector3Int(x, y, 0), new Vector3Int(x + 1, y, 0), new Vector3Int(x, y + 1, 0),
                        new Vector3Int(x + 1, y + 1, 0), rotate90);
                }
                for (var z = 0; z < zSize; z++)
                {
                    var rotate90 = !(z < zSize / 2 ^ y < ySize / 2);
                    SetQuad(new Vector3Int(0, y, z), new Vector3Int(0, y + 1, z), new Vector3Int(0, y, z + 1),
                        new Vector3Int(0, y + 1, z + 1), rotate90);
                }

                for (var x = 0; x < xSize; x++)
                {
                    var rotate90 = x < xSize / 2 ^ y < ySize / 2;
                    SetQuad(new Vector3Int(x, y, zSize), new Vector3Int(x, y + 1, zSize),
                        new Vector3Int(x + 1, y, zSize), new Vector3Int(x + 1, y + 1, zSize), rotate90);
                }

                for (var z = 0; z < zSize; z++)
                {
                    var rotate90 = !(y < ySize / 2 ^ z < zSize / 2);
                    SetQuad(new Vector3Int(xSize, y, z), new Vector3Int(xSize, y, z + 1),
                        new Vector3Int(xSize, y + 1, z), new Vector3Int(xSize, y + 1, z + 1), rotate90);
                }
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
            {
                var rotate90 = !(x < xSize / 2 ^ z < zSize / 2);
                SetQuad(new Vector3Int(x, ySize, z), new Vector3Int(x + 1, ySize, z), new Vector3Int(x, ySize, z + 1), new Vector3Int(x + 1, ySize, z + 1), rotate90);
            }

            return tris;
        }

        private List<int> CreateBottomFace()
        {
            var tris = new List<int>();
            for (var z = 0; z < zSize; z++)
            for (var x = 0; x < xSize; x++)
            {
                var rotate90 = !(x < xSize / 2 ^ z < zSize / 2);
                SetQuad(new Vector3Int(x, 0, z), new Vector3Int(x, 0, z + 1), new Vector3Int(x + 1, 0, z), new Vector3Int(x + 1, 0, z + 1), rotate90);
            }

            return tris;
        }

        private void SetQuad(Vector3Int v00, Vector3Int v01, Vector3Int v10, Vector3Int v11, bool rotate90 = false)
        {
            // Get the indices
            var i00 = vertexXYZToIndex[v00.x, v00.y, v00.z];
            var i01 = vertexXYZToIndex[v01.x, v01.y, v01.z];
            var i10 = vertexXYZToIndex[v10.x, v10.y, v10.z];
            var i11 = vertexXYZToIndex[v11.x, v11.y, v11.z];

            if (!rotate90)
            {
                SetTriangle(i00, i10, i01);
                SetTriangle(i01, i10, i11);
            }
            else
            {
                SetTriangle(i01, i00, i11);
                SetTriangle(i11, i00, i10);
            }
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
        #endregion

        #region Normal Correction
        private void CorrectBottomEdgeNormals()
        {
            var normals = mesh.normals;
            for (var x = 0; x <= xSize; x++)
            {
                for (var z = 0; z <= zSize; z++)
                {
                    // Only looking at edges
                    if (x != 0 && x != xSize && z != 0 && z != zSize) continue;
                    var index = vertexXYZToIndex[x, 0, z];
                    var vertex = vertices[index];
                    normals[index] = BottomEdgeNormal(vertex);
                }
            }
            mesh.normals = normals;
        }
        private void CorrectTopEdgeNormals()
        {
            var normals = mesh.normals;
            for (var x = 0; x <= xSize; x++)
            {
                for (var z = 0; z <= zSize; z++)
                {
                    // Only looking at edges
                    if (x != 0 && x != xSize && z != 0 && z != zSize) continue;
                    var index = vertexXYZToIndex[x, ySize, z];
                    var vertex = vertices[index];
                    normals[index] = TopEdgeNormal(vertex);
                }
            }
            mesh.normals = normals;
        }

        private Vector3 BottomEdgeNormal(Vector3 vertex)
        {
            var innerDifference = CalculateDifferenceVectorFromInnerSurface(vertex.x, vertex.y, vertex.z, roundingRadius);
            
            var vertical = roundingRadius > 0 ? Vector3.down : Vector3.zero;
            
            return (innerDifference.normalized + vertical).normalized;
        }
        private Vector3 TopEdgeNormal(Vector3 vertex)
        {
            var innerDifference = CalculateDifferenceVectorFromInnerSurface(vertex.x, vertex.y, vertex.z, roundingRadius);
            
            return (innerDifference.normalized + Vector3.up).normalized;
        }
        #endregion
    }
}
