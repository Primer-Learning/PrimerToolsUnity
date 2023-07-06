using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    [ExecuteAlways]
    public class StackedAreaNew : MonoBehaviour
    {
        private Transform rootCache;

        public Material material;
        public List<DataTrack> areas = new();

        [Button]
        public void UpdateAreas()
        {
            var min = new Vector3(0, 0, -5);
            var max = new Vector3(6, 12, 5);

            transform.RemoveAllChildren();
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.SetParent(transform);

            var numValuesInShaderFloatArray = 900;
            var pointsPerUnit = 1;
            var xLengthMinusPadding = 2.7f;
            var yLengthMinusPadding = 2.7f;
            var shaderXStep = numValuesInShaderFloatArray / (max.x - min.x);

            plane.transform.localScale = new Vector3 (
                //Stretch along x in proportion to number of values in shader float arrays
                xLengthMinusPadding / (max.x - min.x) * shaderXStep / pointsPerUnit,
                yLengthMinusPadding / (max.y - min.y),
                1
            );

            var meshFilter = plane.GetComponent<MeshFilter>();
            var meshRenderer = plane.GetComponent<MeshRenderer>();

            areas = new List<DataTrack> {
                new() {
                    color = new Color(1, 0, 0),
                    values = new List<float>() { 1f, 2f, 1f, 2f, 1f, 2f },
                },
                new() {
                    color = new Color(0, 0, 1),
                    values = new List<float>() { 1f, 2f, 3f, 4f, 5f, 6f },
                },
            };

            meshFilter.mesh = UpdateMesh(meshFilter.mesh, min, max);
            meshRenderer.material = UpdateShader(new Material(material), min, max, areas);
        }

        private static Mesh UpdateMesh(Mesh plane, Vector3 min, Vector3 max)
        {
            var boundsMin = plane.bounds.center - plane.bounds.extents;
            var boundsMax = plane.bounds.center + plane.bounds.extents;

            // var verts = new Vector3[plane.vertexCount];

            plane.vertices = plane.vertices
                .Select(v => new Vector3(
                    MapFloat(v.x, boundsMin.x, boundsMax.x, min.x, max.x),
                    MapFloat(v.z, boundsMin.z, boundsMax.z, min.y, max.y),
                    0
                ))
                .ToArray();

            plane.RecalculateNormals();
            plane.RecalculateBounds();

            return plane;
        }

        private static Material UpdateShader(Material material, Vector3 min, Vector3 max, IEnumerable<DataTrack> data)
        {
            var numValuesInShaderFloatArray = 900;

            material.SetVector("_VisibleMin", min);
            material.SetVector("_VisibleMax", max);
            material.SetVector("_Min", min);
            material.SetVector("_Max", max);
            material.SetInt("_ValuesCount", numValuesInShaderFloatArray);

            foreach (var (index, area) in data.WithIndex()) {
                var num = index + 1;
                var areaData = new float[numValuesInShaderFloatArray];

                for (var i = 0; i < area.values.Count; i++)
                    areaData[i] = area.values[i];

                Debug.Log(areaData.Join(","));

                material.SetColor($"_Color{num}", area.color);
                material.SetFloatArray($"_FuncValues{num}", areaData.ToList());
                material.SetFloatArray("myDummyName", areaData.ToList());
            }

            return material;
        }

        public static float MapFloat(float value, float fromMin, float fromMax, float toMin, float toMax) {
            return Map01Float(MapFloat01(value, fromMin, fromMax), toMin, toMax);
        }

        public static float MapFloat01(float value, float fromMin, float fromMax) {
            return (value - fromMin) / (fromMax - fromMin);
        }

        public static float Map01Float(float value, float toMin, float toMax) {
            return value * (toMax - toMin) + toMin;
        }
    }
}
