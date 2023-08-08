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

        using var graph =  new Gnome<Graph>("Graph2")
            // We used new Gnome() to get (or create) the root object "Graph"
            // now we only need the component
            .component;

        graph.scale = 0.2f;
        graph.enableZAxis = false;

        yield return graph.GrowFromOrigin(10);
        yield return graph.SetDomain(5);

        foreach (var _ in TestLine(graph))
            yield return _;

        foreach (var _ in TestSurface(graph, cam))
            yield return _;

        foreach (var _ in TestPoint(graph))
            yield return _;

        foreach (var _ in TestArea(graph))
            yield return _;

        foreach (var _ in TestBars(graph))
            yield return _;

        yield return graph.ShrinkToOrigin();
    }

    private static IEnumerable<Tween> TestLine(Graph graph)
    {
        // Blue is disabled when this method ends
        using var blue = graph.AddLine("Blue line");
        blue.width = 0.05f;
        blue.SetColor(PrimerColor.blue);
        blue.SetFunction(x => Mathf.Cos(x * 2) * 5, resolution: 100, xEnd: 5);
        yield return blue.GrowFromStart();

        // Red is disabled when this method ends
        using var red = graph.AddLine("Red line");
        red.width = 0.05f;
        red.SetColor(PrimerColor.red);
        red.SetZIndex(1);
        red.SetData(new float[] { 0, 1, 2, 3 });
        yield return red.GrowFromStart();

        blue.SetFunction(x => Mathf.Sin(x * 2) * 5);
        yield return blue.Transition();

        blue.SetFunction(x => Mathf.Pow(x, 2));
        red.AddPoint(4, 5);

        yield return Parallel(
            blue.Transition(),
            red.Transition()
        );

        foreach (var _ in RunGraphDeformations(graph))
            yield return _;

        yield return blue.ShrinkToEnd();
        yield return red.ShrinkToEnd();
    }

    private static IEnumerable<Tween> TestSurface(Graph graph, CameraRig cam)
    {
        yield return Parallel(
            delayBetweenStarts: 0.1f,
            graph.GrowZAxis(5),
            cam.Travel(
                distance: 4,
                swivel: new Vector3(15f, -25f, 0f)
            )
        );

        using var surface = graph.AddSurface("Surface");

        surface.SetFunction(
            (x, y) => Mathf.Sin(x) + Mathf.Cos(y),
            Vector2Int.one * 20,
            end: Vector2.one * 5
        );

        yield return surface.GrowFromStart();

        surface.SetData(new float[,] {
            { 0, 1, 2, 3, 4 },
            { 1, 2, 3, 4, 5 },
            { 2, 3, 4, 5, 6 },
            { 3, 4, 5, 6, 6 },
            { 4, 5, 6, 6, 5 },
        });
        // Transition will replace the function points with the data leaving resolution at 2x2
        yield return surface.Transition();

        foreach (var _ in RunGraphDeformations(graph))
            yield return _;

        // But we don't care because transitions don't use resolution, they are able smoothly to cut meshes of any resolution
        yield return surface.ShrinkToEnd();

        yield return Parallel(
            delayBetweenStarts: 0.1f,
            graph.ShrinkZAxis(),
            cam.Travel(
                distance: 2.8f,
                swivelOrigin: new Vector3(1.1f, 1.1f, 0f),
                swivel: new Vector3(0f, 0f, 0f)
            )
        );
    }

    private static IEnumerable<Tween> TestPoint(Graph graph)
    {
        var prefab = Prefab.Get("blob_skinned");
        var blob = prefab.GetOrAddComponent<PrimerBlob>();
        blob.GetOrAddComponent<SkinnedMeshRenderer>().sharedMaterial ??= Primer.RendererExtensions.defaultMaterial;

        // Use `AddPoint()` to get the PrimerBlob back instead of the GraphDomain component
        using var pointA = graph.AddTracker("Point A", blob, new Vector3(2, 5));
        yield return pointA.ScaleTo(0.1f, 0);

        using var pointB = graph.AddTracker("Point B", blob, new Vector3(5, 3));
        yield return pointB.ScaleTo(0.1f, 0);

        yield return Parallel(
            Tween.Value(() => pointA.point, new Vector3(6, 1)),
            Tween.Value(() => pointB.point, new Vector3(1, 1))
        );

        foreach (var _ in RunGraphDeformations(graph))
            yield return _;

        yield return Parallel(
            pointA.ScaleTo(0),
            pointB.ScaleTo(0)
        );
    }

    private static IEnumerable<Tween> TestArea(Graph graph)
    {
        using var stackedArea = graph.AddStackedArea("Stacked");

        stackedArea.SetData(
            new float[] { 1, 2, 1, 2 },
            new float[] { 1, 2, 3, 4 }
        );

        yield return stackedArea.GrowFromStart();

        stackedArea.SetData(
            new float[] { 1, 1.5f, 1, 1.5f },
            new float[] { 4, 3, 2, 1 }
        );

        yield return stackedArea.Transition();

        stackedArea.AddArea(0.25f, 0.5f, 0.75f, 1);
        yield return stackedArea.Transition();

        stackedArea.AddData(1, 0.25f, 2);
        yield return stackedArea.Transition();

        foreach (var _ in RunGraphDeformations(graph))
            yield return _;

        yield return stackedArea.ShrinkToEnd();
    }

    private static IEnumerable<Tween> TestBars(Graph graph)
    {
        using var barData = graph.AddBarPlot("Bar data");

        barData.SetData(new float [,] {
            { 1, 2, 1, 2 },
            { 1, 2, 3, 4 },
        });

        yield return barData.GrowFromStart();

        barData.SetData(
            new float[] { 1, 1.5f, 1, 1.5f },
            new float[] { 4, 3, 2, 1 }
        );

        yield return barData.Transition();

        barData.AddStack(0.25f, 0.5f);
        yield return barData.Transition();

        barData.AddData(1, 0.25f, 2);
        yield return barData.Transition();

        foreach (var _ in RunGraphDeformations(graph))
            yield return _;

        yield return barData.ShrinkToEnd();
    }

    private static IEnumerable<Tween> RunGraphDeformations(Graph graph)
    {
        // Domain grows but graph doesn't take more space
        // - ticks shrink together, some are added
        // - dataviz has to shrink
        yield return graph.GrowDomainInSameSpace(5);
        // - ticks walk into the abyss
        yield return graph.ShrinkDomainInSameSpace(5);

        // Domain remains while graph grows
        // - ticks untouched
        // - dataviz has to grow accordingly
        yield return graph.SetGraphScale(1);
        // - ticks untouched
        yield return graph.SetGraphScale(0.2f);

        // Domain grows and graph size grows proportionally
        // - ticks stay in place while some are added
        // - dataviz untouched
        yield return graph.GrowDomain(5);
        // - ticks are eaten by the graph's arrow
        yield return graph.ShrinkDomain(5);
    }
}
