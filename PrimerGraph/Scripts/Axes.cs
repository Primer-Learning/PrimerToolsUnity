using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Axes : PrimerObject
{
    public Graph graph; //Assigned by parent Graph during initialization 
    
    public PrimerObject xAxisMasterContainer; //Assigned in editor. Purpose is to animate all xAxis objects in/out uniformly.
    public PrimerObject yAxisMasterContainer;
    public PrimerObject zAxisMasterContainer;
    //Would use [Serialize Field], but that causes Unity to give a warning that it's never assigned, which bothers me more than namespace stuff at this point.
   
    //Containers for cylinder objects, as a method for aligning objects based on min of range rather than center, which is what the default cylinder primitive does.
    public Transform xAxisRod;
    public Transform yAxisRod;
    public Transform zAxisRod;
    
    //Tics, labels, arrows
    public Tic ticPrefab;
    public PrimerText primerTextPrefab;
    
    public List<Tic> xTics = new List<Tic>();
    internal List<Transform> xArrows = new List<Transform>();
    public PrimerText xAxisLabel;
    
    public List<Tic> yTics = new List<Tic>();
    private List<Transform> yArrows = new List<Transform>();
    public PrimerText yAxisLabel;
    
    public List<Tic> zTics = new List<Tic>();
    private List<Transform> zArrows = new List<Transform>();
    public PrimerText zAxisLabel;

    public static float MapFloat(float value, float fromMin, float fromMax, float toMin, float toMax) {
        return Map01Float(MapFloat01(value, fromMin, fromMax), toMin, toMax);
    }

    public static float MapFloat01(float value, float fromMin, float fromMax) {
        return (value - fromMin) / (fromMax - fromMin);
    }

    public static float Map01Float(float value, float toMin, float toMax) {
        return value * (toMax - toMin) + toMin;
    }

    public void SetUpAxes()
    {
        ////x axis
        
        //Axis rod
        xAxisRod.transform.localPosition = new Vector3 (graph.xAxisOffset, 0f, 0f);
        xAxisRod.transform.localScale = new Vector3 (
            graph.xAxisLength,
            xAxisRod.transform.localScale.y * graph.xAxisThickness,
            xAxisRod.transform.localScale.z * graph.xAxisThickness
        );
        
        //Axis label
        xAxisLabel = Instantiate(primerTextPrefab, xAxisMasterContainer.transform);
        Vector3 xLabelPos = Vector3.zero;
        if (graph.xAxisLabelPos == "along") {
            xLabelPos = new Vector3 (graph.xAxisLength / 2 + graph.xAxisOffset, -2 * graph.ticLabelDistanceVertical, 0f);
        }
        else if (graph.xAxisLabelPos == "end") {
            xLabelPos = new Vector3 (graph.xAxisLength + graph.xAxisOffset + graph.ticLabelDistanceVertical * 1.1f, 0f, 0f);
        }
        xAxisLabel.transform.localPosition = xLabelPos;
        xAxisLabel.tmpro.text = graph.xAxisLabelString;
        xAxisLabel.tmpro.alignment = TextAlignmentOptions.Midline;

        //Axis arrowheads
        switch (graph.arrows)
        {
            case "both":
                xArrows.Add(Instantiate(graph.arrowPrefab, new Vector3 (graph.xAxisLength + graph.xAxisOffset, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), xAxisMasterContainer.transform).transform);
                xArrows.Add(Instantiate(graph.arrowPrefab, new Vector3 (graph.xAxisOffset, 0f, 0f), Quaternion.Euler(0f, -90f, 0f), xAxisMasterContainer.transform).transform);
                //Acually, should be localPosition
                xArrows[0].transform.localPosition = xArrows[0].transform.position;
                xArrows[1].transform.localPosition = xArrows[1].transform.position;
                xArrows[0].transform.localRotation = xArrows[0].transform.rotation;
                xArrows[1].transform.localRotation = xArrows[1].transform.rotation;
                break;
            case "positive":
                xArrows.Add(Instantiate(graph.arrowPrefab, new Vector3 (graph.xAxisLength + graph.xAxisOffset, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), xAxisMasterContainer.transform).transform);
                xArrows[0].transform.localPosition = xArrows[0].transform.position;
                xArrows[0].transform.localRotation = xArrows[0].transform.rotation;
                break;
            case "neither":
                break;
            default:
                Debug.LogWarning("Graph arrow setting undefined. Defaulting to none.");
                break;
        }
        HandleTicsX();

        if (graph.xHidden) { xAxisMasterContainer.transform.localScale = new Vector3 (0f, 0f, 0f); }

        
        ////y axis

        //Axis rod
        yAxisRod.transform.localPosition = new Vector3(0f, graph.yAxisOffset, 0f);
        yAxisRod.transform.localScale = new Vector3(
            yAxisRod.transform.localScale.x * graph.yAxisThickness,
            graph.yAxisLength,
            yAxisRod.transform.localScale.z * graph.yAxisThickness
        );

        //Axis label
        yAxisLabel = Instantiate(primerTextPrefab, yAxisMasterContainer.transform);
        Vector3 yLabelPos = Vector3.zero;
        if (graph.yAxisLabelPos == "along")
        {
            yLabelPos = new Vector3(-2 * graph.ticLabelDistanceVertical, graph.yAxisLength / 2 + graph.yAxisOffset, 0f);
        }
        else if (graph.yAxisLabelPos == "end")
        {
           yLabelPos = new Vector3(0f, graph.yAxisLength + graph.yAxisOffset + graph.ticLabelDistanceVertical * 1.1f, 0f);
           yAxisLabel.tmpro.alignment = TextAlignmentOptions.Baseline;
        }
        yAxisLabel.transform.localPosition = yLabelPos;//yAxisLabel.transform.position;
        yAxisLabel.tmpro.text = graph.yAxisLabelString;

        //Axis arrowheads
        switch (graph.arrows)
        {
            case "both":
                yArrows.Add(Instantiate(graph.arrowPrefab, new Vector3(0f, graph.yAxisLength + graph.yAxisOffset, 0f), Quaternion.Euler(-90f, 0f, 0f), yAxisMasterContainer.transform).transform);
                yArrows.Add(Instantiate(graph.arrowPrefab, new Vector3(0f, graph.yAxisOffset, 0f), Quaternion.Euler(90f, 0f, 0f), yAxisMasterContainer.transform).transform);
                //Acually, should be localPosition
                yArrows[0].transform.localPosition = yArrows[0].transform.position;
                yArrows[1].transform.localPosition = yArrows[1].transform.position;
                yArrows[0].transform.localRotation = yArrows[0].transform.rotation;
                yArrows[1].transform.localRotation = yArrows[1].transform.rotation;
                break;
            case "positive":
                yArrows.Add(Instantiate(graph.arrowPrefab, new Vector3(0f, graph.yAxisLength + graph.yAxisOffset, 0f), Quaternion.Euler(-90f, 0f, 0f), yAxisMasterContainer.transform).transform);
                yArrows[0].transform.localPosition = yArrows[0].transform.position;
                yArrows[0].transform.localRotation = yArrows[0].transform.rotation;
                break;
            case "neither":
                break;
            default:
                Debug.LogWarning("Graph arrow setting undefined. Defaulting to none.");
                break;
        }
        HandleTicsY();

        if (graph.yHidden) { yAxisMasterContainer.transform.localScale = new Vector3(0f, 0f, 0f); }


        ////z axis

        //Axis rod
        zAxisRod.transform.localPosition = new Vector3(0f, 0f, graph.zAxisOffset);
        zAxisRod.transform.localScale = new Vector3(
            zAxisRod.transform.localScale.x * graph.zAxisThickness,
            zAxisRod.transform.localScale.y * graph.zAxisThickness,
            graph.zAxisLength);

        //Axis label
        zAxisLabel = Instantiate(primerTextPrefab, zAxisMasterContainer.transform);
        Vector3 zLabelPos = Vector3.zero;
        if (graph.zAxisLabelPos == "along")
        {
            zLabelPos = new Vector3(0f, -2 * graph.ticLabelDistanceVertical, graph.zAxisLength / 2 + graph.zAxisOffset);
        }
        else if (graph.zAxisLabelPos == "end")
        {
            zLabelPos = new Vector3(0f, 0f, graph.zAxisLength + graph.zAxisOffset + graph.ticLabelDistanceVertical * 1.1f);
        }
        zAxisLabel.transform.localPosition = zLabelPos;
        zAxisLabel.tmpro.text = graph.zAxisLabelString;
        zAxisLabel.tmpro.alignment = TextAlignmentOptions.Midline;

        //Axis arrowheads
        switch (graph.arrows)
        {
            case "both":
                zArrows.Add(Instantiate(graph.arrowPrefab, new Vector3(0f, 0f, graph.zAxisLength + graph.zAxisOffset), Quaternion.Euler(0f, 0f, 0f), zAxisMasterContainer.transform).transform);
                zArrows.Add(Instantiate(graph.arrowPrefab, new Vector3(0f, 0f, graph.zAxisOffset), Quaternion.Euler(0f, -180f, 0f), zAxisMasterContainer.transform).transform);
                zArrows[0].transform.localPosition = zArrows[0].transform.position;
                zArrows[1].transform.localPosition = zArrows[1].transform.position;
                zArrows[0].transform.localRotation = zArrows[0].transform.rotation;
                zArrows[1].transform.localRotation = zArrows[1].transform.rotation;
                break;
            case "positive":
                zArrows.Add(Instantiate(graph.arrowPrefab, new Vector3(0f, 0f, graph.zAxisLength + graph.zAxisOffset), Quaternion.Euler(0f, 0f, 0f), zAxisMasterContainer.transform).transform);
                zArrows[0].transform.localPosition = zArrows[0].transform.position;
                zArrows[0].transform.localRotation = zArrows[0].transform.rotation;
                break;
            case "neither":
                break;
            default:
                Debug.LogWarning("Graph arrow setting undefined. Defaulting to none.");
                break;
        }
        HandleTicsZ();
        
        //We use right-handed systems around these parts
        if (graph.rightHanded) {
            zAxisMasterContainer.SetIntrinsicScale(new Vector3(1f, 1f, -1f));
            zAxisMasterContainer.transform.localScale = zAxisMasterContainer.GetIntrinsicScale();
        }

        if (graph.zHidden) { zAxisMasterContainer.transform.localScale = new Vector3(0f, 0f, 0f); }
    }

    public void ShowAxisX(float duration = 0.5f)
    {
        xAxisMasterContainer.ScaleUpFromZero(duration: duration);
        graph.xHidden = false;
    }
    public void HideAxisX(float duration = 0.5f)
    {
        xAxisMasterContainer.ScaleDownToZero(duration: duration);
        graph.xHidden = true;
    }
    
    public void ShowAxisY(float duration = 0.5f)
    {
        yAxisMasterContainer.ScaleUpFromZero(duration: duration);
        graph.yHidden = false;
    }
    public void HideAxisY(float duration = 0.5f)
    {
        yAxisMasterContainer.ScaleDownToZero(duration: duration);
        graph.yHidden = true;
    }

    public void ShowAxisZ(float duration = 0.5f)
    {
        zAxisMasterContainer.ScaleUpFromZero(duration: duration);
        graph.zHidden = false;
    }
    public void HideAxisZ(float duration = 0.5f)
    {
        zAxisMasterContainer.ScaleDownToZero(duration: duration);
        graph.zHidden = true;
    }

    public void HandleTicsX()
    {
        if (graph.xTicStep <= 0) {return;}
        if (!graph.manualTicMode) {
            List<Tic> toRemove = new List<Tic>(xTics);
            //Make missing tics
            //Above zero
            float i = graph.xTicStep;
            while (i <= graph.xMax)
            {
                bool exists = false;
                foreach (Tic tic in xTics)
                {
                    if (tic.value == i) { 
                        exists = true; 
                        toRemove.Remove(tic);
                    }
                }
                if (exists == false)
                {
                    Tic newTic = Instantiate(ticPrefab, xAxisMasterContainer.transform);
                    newTic.graph = this.graph;
                    newTic.axis = "x";
                    newTic.value = i;
                    newTic.SetPosition();
                    xTics.Add(newTic);
                    newTic.MakeLabel();
                    newTic.ScaleUpFromZero();
                }
                i += graph.xTicStep;
            }
            //Below zero
            i = -graph.xTicStep;
            while (i >= graph.xMin)
            {
                bool exists = false;
                foreach (Tic tic in xTics)
                {
                    if (tic.value == i) { 
                        exists = true; 
                        toRemove.Remove(tic);
                    }
                }
                if (exists == false)
                {
                    Tic newTic = Instantiate(ticPrefab, xAxisMasterContainer.transform);
                    newTic.graph = this.graph;
                    newTic.axis = "x";
                    newTic.value = i;
                    newTic.SetPosition();
                    xTics.Add(newTic);
                    newTic.MakeLabel();
                    newTic.ScaleUpFromZero();
                }
                i -= graph.xTicStep;
            }
            foreach (Tic tic in toRemove) {
                xTics.Remove(tic);
                //Hacky way of destroying ones that are gone without a coroutine
                if (tic.transform.localScale == Vector3.zero) {
                    Destroy(tic);
                }
                else {
                    tic.ScaleDownToZero();
                }
            }
        }
        else { //Manual tic mode
            List<Tic> toKeep = new List<Tic>();
            foreach (KeyValuePair<float, string> entry in graph.manualTicsX) {
                bool exists = false;
                foreach (Tic tic in xTics)
                {
                    if (tic.value == entry.Key) {
                        exists = true;
                        toKeep.Add(tic);
                    }
                }
                if (exists == false)
                {
                    Tic newTic = Instantiate(ticPrefab, xAxisMasterContainer.transform);
                    Destroy(newTic.transform.Find("cylinder").gameObject);
                    newTic.graph = this.graph;
                    newTic.axis = "x";
                    newTic.value = entry.Key;
                    newTic.SetPosition();
                    xTics.Add(newTic);
                    newTic.MakeLabel(entry.Value);
                    newTic.ScaleUpFromZero();
                    toKeep.Add(newTic);
                }
            }
            for (int i = xTics.Count - 1; i >= 0; i--) {
                Tic tic = xTics[i];
                if (!toKeep.Contains(tic)) {
                    xTics.Remove(tic);
                    tic.Disappear();
                }
            }
        }
    }

    public void HandleTicsY()
    {
        if (graph.yTicStep <= 0) {return;}
        // if (!graph.manualTicMode) {
            List<Tic> toRemove = new List<Tic>(yTics);
            //Make missing tics
            //Above zero
            float i = graph.yTicStep;
            while (i <= graph.yMax)
            {
                bool exists = false;
                foreach (Tic tic in yTics)
                {
                    if (tic.value == i) { 
                        exists = true; 
                        toRemove.Remove(tic);
                    }
                }
                if (exists == false)
                {
                    Tic newTic = Instantiate(ticPrefab, yAxisMasterContainer.transform);
                    newTic.graph = this.graph;
                    newTic.axis = "y";
                    newTic.value = i;
                    newTic.SetPosition();
                    yTics.Add(newTic);
                    newTic.MakeLabel();
                    newTic.ScaleUpFromZero();
                }
                i += graph.yTicStep;
            }
            //Below zero
            i = -graph.yTicStep;
            while (i >= graph.yMin)
            {
                bool exists = false;
                foreach (Tic tic in yTics)
                {
                    if (tic.value == i) { 
                        exists = true; 
                        toRemove.Remove(tic);
                    }
                }
                if (exists == false)
                {
                    Tic newTic = Instantiate(ticPrefab, yAxisMasterContainer.transform);
                    newTic.graph = this.graph;
                    newTic.axis = "y";
                    newTic.value = i;
                    newTic.SetPosition();
                    yTics.Add(newTic);
                    newTic.MakeLabel();
                    newTic.ScaleUpFromZero();
                }
                i -= graph.yTicStep;
            }
            foreach (Tic tic in toRemove) {
                yTics.Remove(tic);
                //Hacky way of destroying ones that are gone without a coroutine
                if (tic.transform.localScale == Vector3.zero) {
                    Destroy(tic);
                }
                else {
                    tic.ScaleDownToZero();
                }
            }
        // }
        // else { //Manual tic mode
        //     foreach (KeyValuePair<float, string> entry in graph.manualTicsY) {
        //         bool exists = false;
        //         foreach (Tic tic in xTics)
        //         {
        //             if (tic.value == entry.Key) { exists = true; }
        //         }
        //         if (exists == false)
        //         {
        //             Tic newTic = Instantiate(ticPrefab, xAxisMasterContainer.transform);
        //             newTic.graph = this.graph;
        //             newTic.axis = "y";
        //             newTic.value = entry.Key;
        //             newTic.SetPosition();
        //             yTics.Add(newTic);
        //             newTic.MakeLabel(entry.Value);
        //             newTic.ScaleUpFromZero();
        //         }
        //     }
        // }
    }

    private void HandleTicsZ()
    {
        if (graph.zTicStep <= 0) {return;}
        //Make missing tics
        //Above zero
        float i = graph.zTicStep;
        while (i <= graph.zMax)
        {
            bool exists = false;
            foreach (Tic tic in zTics)
            {
                if (tic.value == i) { exists = true; }
            }
            if (exists == false)
            {
                Tic newTic = Instantiate(ticPrefab, zAxisMasterContainer.transform);
                newTic.graph = this.graph;
                newTic.axis = "z";
                newTic.value = i;
                newTic.SetPosition();
                zTics.Add(newTic);
                newTic.MakeLabel();
                newTic.ScaleUpFromZero();
            }
            i += graph.zTicStep;
        }
        //Below zero
        i = -graph.zTicStep;
        while (i >= graph.zMin)
        {
            bool exists = false;
            foreach (Tic tic in zTics)
            {
                if (tic.value == i) { exists = true; }
            }
            if (exists == false)
            {
                Tic newTic = Instantiate(ticPrefab, zAxisMasterContainer.transform);
                newTic.graph = this.graph;
                newTic.axis = "z";
                newTic.value = i;
                newTic.SetPosition();
                zTics.Add(newTic);
                newTic.MakeLabel();
                newTic.ScaleUpFromZero();
            }
            i -= graph.zTicStep;
        }
    }
}