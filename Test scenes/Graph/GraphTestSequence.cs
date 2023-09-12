using System.Collections.Generic;
using Primer;
using Primer.Animation;
using Primer.Graph;
using Primer.Scene;
using Primer.Timeline;
using UnityEngine;

public class GraphTestSequence : Sequence
{
    public override async IAsyncEnumerator<Tween> Define()
    {
        var cam = FindObjectOfType<CameraRig>();
        
        cam.Travel(
            distance: 3.8f,
            swivelOrigin: new Vector3(0.6f, 0.9f, 0f),
            swivel: new Vector3(0f, 0f, 0f)
        ).Apply();

        using var graph = new SimpleGnome("Graph3", "Graph").transform.GetComponent<Graph3>();
        
        graph.transform.localPosition = new Vector3(-3, 0, 0);
        
        #region Axes manipulations alone
        graph.xAxis.length = 3;
        graph.yAxis.length = 2;
        graph.zAxis.length = 0;
        graph.xAxis.max = 10;
        graph.xAxis.ticStep = 2;
        yield return graph.Appear();
        graph.xAxis.length = 2;
        yield return graph.Transition();

        graph.xAxis.max = 20;
        graph.xAxis.ticStep = 4;
        yield return graph.Transition();
        graph.xAxis.max = 10;
        graph.xAxis.ticStep = 2;
        yield return graph.Transition();
        #endregion
        
        foreach (var _ in RunGraphDeformations(graph))
            yield return _;
        
        foreach (var _ in TestLine(graph))
            yield return _;
        
        foreach (var _ in TestArea(graph))
            yield return _;
        
        yield return graph.Disappear();

        // foreach (var _ in TestSurface(graph, cam))
        //     yield return _;
        //
        // foreach (var _ in TestPoint(graph))
        //     yield return _;
        //
        //
        // foreach (var _ in TestBars(graph))
        //     yield return _;
        //
        // yield return graph.ShrinkToOrigin() with { name = "Graph goes" };
    }

    private IEnumerable<Tween> TestLine(Graph3 graph)
    {
        PushClipColor(PrimerColor.red);
    
        // Blue is disabled when this method ends
        using var blue = graph.AddLine("Blue line");
        blue.width = 0.05f;
        blue.SetColor(PrimerColor.blue);
        blue.SetFunction(x => Mathf.Cos(x * 2) * 5, numPoints: 100, xEnd: 5);
        yield return blue.GrowFromStart() with { name = "Blue line" };
    
        // Red is disabled when this method ends
        using var red = graph.AddLine("Red line");
        red.width = 0.05f;
        red.SetColor(PrimerColor.red);
        red.SetZIndex(1);
        red.SetData(new float[] { 0, 1, 2, 3 });
        yield return red.GrowFromStart() with { name = "Red line" };
    
        blue.SetFunction(x => Mathf.Sin(x * 2) * 5);
        yield return blue.Transition() with { name = "Blue transition" };
    
        blue.SetFunction(x => Mathf.Pow(x, 2));
        red.AddPoint(3, 4);
    
        yield return Parallel(
            blue.Transition(),
            red.Transition()
        ) with { name = "Lines transition" };
    
        foreach (var _ in RunGraphDeformations(graph))
            yield return _;
        
        yield return Parallel(
            blue.ShrinkToEnd(),
            red.ShrinkToEnd()
        ) with { name = "Lines go" };
    
        PopClipColor();
    }

    // private IEnumerable<Tween> TestSurface(Graph graph, CameraRig cam)
    // {
    //     PushClipColor(PrimerColor.green);
    //
    //     graph.enableZAxis = true;
    //     yield return Parallel(
    //         delayBetweenStarts: 0.1f,
    //         graph.z.GrowFromOrigin(5),
    //         cam.Travel(
    //             distance: 4,
    //             swivel: new Vector3(15f, -25f, 0f)
    //         )
    //     ) with { name = "Adjust camera" };
    //
    //     using var surface = graph.AddSurface("Surface");
    //
    //     surface.SetFunction(
    //         (x, y) => Mathf.Sin(x) + Mathf.Cos(y),
    //         Vector2Int.one * 20,
    //         end: Vector2.one * 5
    //     );
    //
    //     yield return surface.GrowFromStart() with { name = "Surface appears" };
    //
    //     surface.SetData(new float[,] {
    //         { 0, 1, 2, 3, 4 },
    //         { 1, 2, 3, 4, 5 },
    //         { 2, 3, 4, 5, 6 },
    //         { 3, 4, 5, 6, 6 },
    //         { 4, 5, 6, 6, 5 },
    //     });
    //     // Transition will replace the function points with the data leaving resolution at 2x2
    //     yield return surface.Transition() with { name = "Surface transition" };
    //
    //     foreach (var _ in RunGraphDeformations(graph))
    //         yield return _;
    //
    //     // But we don't care because transitions don't use resolution, they are able smoothly to cut meshes of any resolution
    //     yield return surface.ShrinkToEnd() with { name = "Surfac goes" };
    //
    //     yield return Parallel(
    //         delayBetweenStarts: 0.1f,
    //         graph.z.ShrinkToOrigin(),
    //         cam.Travel(
    //             distance: 2.8f,
    //             swivelOrigin: new Vector3(1.1f, 1.1f, 0f),
    //             swivel: new Vector3(0f, 0f, 0f)
    //         )
    //     ) with { name = "Restore camera" };
    //
    //     PopClipColor();
    // }

    // private IEnumerable<Tween> TestPoint(Graph graph)
    // {
    //     PushClipColor(PrimerColor.blue);
    //
    //     var prefab = Prefab.Get("blob_skinned");
    //     var blob = prefab.GetOrAddComponent<PrimerBlob>();
    //     blob.GetOrAddComponent<SkinnedMeshRenderer>().sharedMaterial ??= Primer.RendererExtensions.defaultMaterial;
    //
    //     // Use `AddPoint()` to get the PrimerBlob back instead of the GraphDomain component
    //     using var pointA = graph.AddTracker("Point A", blob, new Vector3(2, 5));
    //     yield return pointA.ScaleTo(0.1f, 0) with { name = "Point A" };
    //
    //     using var pointB = graph.AddTracker("Point B", blob, new Vector3(5, 3));
    //     yield return pointB.ScaleTo(0.1f, 0) with { name = "Point B" };
    //
    //     yield return Parallel(
    //         Tween.Value(
    //             v => pointA.point = v,
    //             () => pointA.point,
    //             () => new Vector3(6, 1)),
    //         Tween.Value(
    //             v => pointB.point = v,
    //             () => pointB.point,
    //             () => new Vector3(1, 1))
    //     ) with { name = "Move points" };
    //
    //     foreach (var _ in RunGraphDeformations(graph))
    //         yield return _;
    //
    //     yield return Parallel(
    //         pointA.ScaleTo(0),
    //         pointB.ScaleTo(0)
    //     ) with { name = "Points go" };
    //
    //     PopClipColor();
    // }

    private IEnumerable<Tween> TestArea(Graph3 graph)
    {
        PushClipColor(PrimerColor.purple);
    
        using var stackedArea = graph.AddStackedArea("Stacked");
    
        stackedArea.SetData(
            new float[] { 1, 2, 1, 2 },
            new float[] { 1, 2, 3, 4 }
        );
    
        yield return stackedArea.GrowFromStart() with { name = "Area appears" };
    
        stackedArea.SetData(
            new float[] { 1, 1.5f, 1, 1.5f },
            new float[] { 4, 3, 2, 1 }
        );
    
        yield return stackedArea.Transition() with { name = "Area transition" };
    
        stackedArea.AddArea(0.25f, 0.5f, 0.75f, 1);
        yield return stackedArea.Transition() with { name = "Add area" };
    
        stackedArea.AddData(1, 0.25f, 2);
        yield return stackedArea.Transition() with { name = "Add data" };
    
        foreach (var _ in RunGraphDeformations(graph))
            yield return _;
    
        yield return stackedArea.ShrinkToEnd() with { name = "Area goes" };
    
        PopClipColor();
    }
    //
    // private IEnumerable<Tween> TestBars(Graph graph)
    // {
    //     PushClipColor(PrimerColor.yellow);
    //
    //     using var barData = graph.AddBarPlot("Bar data");
    //
    //     barData.SetData(new float [,] {
    //         { 1, 2, 1, 2 },
    //         { 1, 2, 3, 4 },
    //     });
    //
    //     yield return barData.GrowFromStart() with { name = "Bars appear" };
    //
    //     barData.SetData(
    //         new float[] { 1, 1.5f, 1, 1.5f },
    //         new float[] { 4, 3, 2, 1 }
    //     );
    //
    //     yield return barData.Transition() with { name = "Bars transition" };
    //
    //     barData.AddStack(0.25f, 0.5f);
    //     yield return barData.Transition() with { name = "Add stack" };
    //
    //     barData.AddData(1, 0.25f, 2);
    //     yield return barData.Transition() with { name = "Add data" };
    //
    //     foreach (var _ in RunGraphDeformations(graph))
    //         yield return _;
    //
    //     yield return barData.ShrinkToEnd() with { name = "Bars go" };
    //
    //     PopClipColor();
    // }
    //
    private IEnumerable<Tween> RunGraphDeformations(Graph3 graph)
    {
        PushClipColor(PrimerColor.orange);
        
        graph.xAxis.max = 15;
        yield return graph.Transition();
        graph.xAxis.max = 10;
        yield return graph.Transition();
    
        graph.yAxis.length = 3;
        yield return graph.Transition();
        graph.yAxis.length = 2;
        yield return graph.Transition();
    
        graph.xAxis.max = 15;
        graph.xAxis.length = 3;
        yield return graph.Transition();
        graph.xAxis.max = 10;
        graph.xAxis.length = 2;
        yield return graph.Transition();
    
        PopClipColor();
    }
}
