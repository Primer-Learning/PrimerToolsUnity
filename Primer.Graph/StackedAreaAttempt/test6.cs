using System.Collections.Generic;
using System.Linq;
using Primer;
using Primer.Graph;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class test6 : MonoBehaviour
{
    class MyGraph : IMyGraph
    {
        public float xMin { get; set; }
        public float xMax { get; set; }
        public float yMin { get; set; }
        public float yMax { get; set; }
        public float xLengthMinusPadding { get; set; }
        public float yLengthMinusPadding { get; set; }
    }

    [Button]
    private void Start()
    {
        transform.RemoveAllChildren();

        var min = new Vector3 (0, 0, -5);
        var max = new Vector3 (6, 12, 5);
        var material = (Material)Resources.Load("StackedAreas", typeof(Material));
        var stackedAreaPlot = this.GetOrAddComponent<StackedAreaData>();

        var template = GameObject.CreatePrimitive(PrimitiveType.Plane);
        template.GetComponent<MeshCollider>().DisposeComponent();
        var plane = Instantiate(template, transform);
        template.Dispose();

        stackedAreaPlot.planeMeshFilter = plane.GetOrAddComponent<MeshFilter>();
        stackedAreaPlot.planeMeshFilter.mesh = CloneMesh(plane.GetComponent<MeshFilter>().sharedMesh);
        stackedAreaPlot.planeRenderer = plane.GetOrAddComponent<MeshRenderer>();
        stackedAreaPlot.planeRenderer.material = material;
        stackedAreaPlot.mins = min;
        stackedAreaPlot.maxs = max;
        stackedAreaPlot.pointsPerUnit = 1;
        stackedAreaPlot.plot = new MyGraph {
            xMin = min.x,
            xMax = max.x,
            yMin = min.y,
            yMax = max.y,
            xLengthMinusPadding = 1f,
            yLengthMinusPadding = 1f,
        };

        stackedAreaPlot.RefreshData();
        stackedAreaPlot.UpdateVisibleRange(stackedAreaPlot.mins, stackedAreaPlot.maxs);

        stackedAreaPlot.SetFunctions(
            Enumerable.Range(0, 10).Select(x => (float)x).ToList(),
            Enumerable.Range(0, 10).Select(x => x % 2 == 0 ? 1f : 0).ToList()
        );

        stackedAreaPlot.SetColors(
            Color.red,
            Color.blue
        );

        stackedAreaPlot.AnimateX();
    }

    public Mesh CloneMesh(Mesh originalMesh)
    {
        return new Mesh {
            vertices = originalMesh.vertices,
            triangles = originalMesh.triangles,
            normals = originalMesh.normals,
            colors = originalMesh.colors,
            uv = originalMesh.uv,
            uv2 = originalMesh.uv2,
            tangents = originalMesh.tangents,
            bindposes = originalMesh.bindposes,
            boneWeights = originalMesh.boneWeights,
        };
    }
}
