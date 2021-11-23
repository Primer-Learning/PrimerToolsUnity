using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class SampleGraphSceneDirector : Director
{
    //These are instantiated from prefabs linked to Director class
    private Graph graph;

    private PrimerText text1;

    protected override void Awake() {
        base.Awake();
        //Do initialization stuff here
        //e.g., set object scale to zero if they're going to scale in
        camRig.SetUp(solidColor: true);
        camRig.cam.transform.localPosition = new Vector3(0, 0, -25);
        camRig.transform.localPosition = 3 * Vector3.up;
        camRig.transform.localRotation = Quaternion.Euler(16, 0, 0);

        text1 = Instantiate(textPrefab, camRig.transform);
        text1.transform.localScale = Vector3.zero;
    }

    //Define event actions
    IEnumerator Appear() { 
        graph = Instantiate(graphPrefab);
        graph.transform.localPosition = new Vector3(-6, 0, 0);
        graph.transform.localRotation = Quaternion.Euler(0, 0, 0); 
        graph.Initialize(
            xTicStep: 2,
            xMax: 10,
            yMax: 12,
            zTicStep: 5,
            zMin: -5,
            zMax: 5,
            xAxisLength: 3,
            yAxisLength: 3,
            zAxisLength: 2,
            scale: 2, //True length is the axis length times scale. Scale controls thickness
            xAxisLabelPos: "along"
        );
        graph.ScaleUpFromZero();
        yield return new WaitForSeconds(1);

        text1.tmpro.text = "Sample text";
        //text1.transform.parent = graph.transform;
        text1.tmpro.alignment = TextAlignmentOptions.Center;
        text1.transform.localPosition = new Vector3(4.5f, 2, 0);
        text1.SetIntrinsicScale(1f);
        text1.ScaleUpFromZero();

    }

    IEnumerator CurveExamples() {
        StartCoroutine(ChangeText(text1, "Curves"));
        float ExampleFunctionCurve(float x) => x * x / 10;
        float ExampleFunctionCurve2(float x) => 5 + 4 * Mathf.Sin(x);
        CurveData efc = graph.AddCurve(ExampleFunctionCurve, "EFC");
        efc.DrawLineAnimation(ease: EaseMode.Cubic, duration: 0.5f);
        yield return new WaitForSeconds(1);
        
        //Curves defined by data just fill the x range.
        //So make sure your data and the graph scale/units match.
        List<int> ExampleDataCurve = new List<int>() {
            5, 6, 5, 3, 6, 4
        };
        CurveData edc = graph.AddCurve(ExampleDataCurve, "EDC");
        edc.SetColor(new Color(1, 0, 0, 1));
        edc.DrawLineAnimation(duration: 0.5f);
        yield return new WaitForSeconds(1);

        efc.AnimateToNewCurve(ExampleFunctionCurve2, duration: 0.5f);
        yield return new WaitForSeconds(1);

        //Graphs can change range, but curves don't currently adjust domain properly
        graph.ChangeRangeX(0, 6);
        yield return new WaitForSeconds(1);

        efc.WipeCurveAnimation();
        yield return new WaitForSeconds(1f);
        edc.WipeCurveAnimation();
        yield return new WaitForSeconds(1f);
    }

    IEnumerator SurfaceExamples() {
        StartCoroutine(ChangeText(text1, "Surfaces"));
        float ExampleSurface(float x, float z) => 4 - x / 10f - z;
        SurfaceData es = graph.AddSurface(ExampleSurface, "ES");
        es.AnimateX(duration: 0.5f);
        yield return new WaitForSeconds(2);

        es.WipeSurfaceX();
    }

    IEnumerator PointExample() {
        StartCoroutine(ChangeText(text1, "Points"));
        Vector3 pointPos = new Vector3 (2, 5, 0);
        PointController point = graph.pointData.AddPoint(pointPos, "point", scale: 0.3f);
        point.ActivatePoint(duration: 0.5f);

        yield return new WaitForSeconds(1);

        point.MovePoint(new Vector3(6, 3, 4));

        yield return new WaitForSeconds(1);
        point.DeactivatePoint(duration: 0.5f);
    }

    IEnumerator StackedAreasExample() {
        //Stacked areas are pretty janky, tbh

        StartCoroutine(ChangeText(text1, "Stacked Areas"));
        List<float> func1 = new List<float>() {1f, 2f, 1f, 2f, 1f, 2f};
        List<float> func2 = new List<float>() {1f, 2f, 3f, 4f, 5f, 6f};
        StackedAreaData stackedArea = graph.AddStackedArea();
        stackedArea.SetFunctions(func1, func2);
        var c1 = new Color(0, 0, 1, 0);
        var c2 = new Color(1, 0, 0, 0);
        stackedArea.SetColors(c1, c2);

        stackedArea.AnimateX();
        yield return null;
    }
    
    IEnumerator Disappear() {
        graph.ScaleDownToZero();
        text1.ScaleDownToZero();
        yield return new WaitForSeconds(1);
    }

    //Example of a custom IEnumerator for kicking off common animations you might not want
    //to wait for during SceneBlock IEnumerators
    IEnumerator ChangeText(PrimerText t, string newString) {
        t.ScaleDownToZero();
        yield return new WaitForSeconds(0.5f);
        t.tmpro.text = newString;
        t.ScaleUpFromZero();
    }
    
    //Construct schedule
    protected override void DefineSchedule() {
        new SceneBlock(0f, Appear);
        new SceneBlock(2f, CurveExamples, flexible: true);
        //When flexible is true, the following starts right after StopWaiting is called.
        new SceneBlock(8f, SurfaceExamples);
        new SceneBlock(13f, PointExample);
        new SceneBlock(17f, StackedAreasExample);
        new SceneBlock(20f, Disappear);
    }
}
