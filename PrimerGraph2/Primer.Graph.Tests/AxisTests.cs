using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Primer.Graph;
using UnityEngine;
using UnityEngine.TestTools;

public class AxisTests
{
    [Test]
    public void AxisTestSmallTicStep() {
        var axis = createAxis();
        axis.ticStep = 0.01f;
        axis.UpdateTics();
    }

    Axis2 createAxis() {
        var graphGameObject = new GameObject();
        var graph = graphGameObject.AddComponent<Graph2>();

        graph.primerTextPrefab = new GameObject().AddComponent<PrimerText2>();
        graph.ticPrefab = new GameObject().AddComponent<Tic2>();

        var axisGameObject = new GameObject {
            transform = {
                parent = graphGameObject.transform
            }
        };

        // Disable it so it doesn't generate children on instantiation
        axisGameObject.SetActive(false);

        var axis = axisGameObject.AddComponent<Axis2>();
        axis.rod = new GameObject().transform;

        return axis;
    }
}
