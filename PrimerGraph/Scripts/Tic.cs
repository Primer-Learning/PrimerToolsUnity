using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tic : PrimerObject
{
    //Tics to know where they are and adjust themselves
    public float value;
    public string axis;
    public Graph graph;
    [SerializeField] Transform cylinder = null;

    public TextMeshPro primerTextPrefab;

    public TextMeshPro label;

    public void SetPosition() 
    {
        if (this.graph == null) { Debug.LogError("Tic has no graph assigned"); }
        if (this.axis == "x")
        {
            float positionInGraph = this.value * graph.xLengthMinusPadding / (graph.xMax - graph.xMin); 
            transform.localPosition = new Vector3(positionInGraph, 0f, 0f);
        }

        if (this.axis == "y")
        {
            float positionInGraph = this.value * graph.yLengthMinusPadding / (graph.yMax - graph.yMin);
            transform.localPosition = new Vector3(0f, positionInGraph, 0f);

            cylinder.localRotation = Quaternion.Euler(0f, 0f, -90f);
        }

        if (this.axis == "z")
        {
            float positionInGraph = this.value * graph.zLengthMinusPadding / (graph.zMax - graph.zMin);
            transform.localPosition = new Vector3(0f, 0f, positionInGraph);
        }
    }

    public void MakeLabel()
    {
        label = Instantiate(primerTextPrefab, this.transform);
        label.text = this.value.ToString();
        AlignLabel();
    }
    public void MakeLabel(string labelText)
    {
        label = Instantiate(primerTextPrefab, this.transform);
        label.text = labelText;
        AlignLabel();
    }
    private void AlignLabel() {
        Vector3 pos = Vector3.zero;
        switch (this.axis)
        {
            case "x":
                pos = new Vector3 (0f, -graph.ticLabelDistanceVertical, 0f);
                break;
            case "y":
                pos = new Vector3 (-graph.ticLabelDistanceHorizontal, 0f, 0f);
                label.alignment = TextAlignmentOptions.MidlineRight;
                break;
            case "z":
                pos = new Vector3 (0f, -graph.ticLabelDistanceVertical, 0f);
                break;
            default:
                Debug.LogError("Tic axis not defined");
                break;
        }        
        label.transform.localPosition = pos;
    }
}
