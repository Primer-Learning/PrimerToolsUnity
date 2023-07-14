using Primer.Graph;
using Primer.Shapes;
using Primer.Simulation;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(TernaryPlot))]
public abstract class TernaryVectorFieldPlotter : MonoBehaviour
{
    public bool liveUpdate = false;
    public int automaticPointIncrements = 3;
    public float baseArrowLength = 1;
    public float baseArrowThickness = 1;

    public void OnValidate()
    {
        if (liveUpdate)
            PlotArrows();
    }

    public void TriggeredPlotArrows()
    {
        if (liveUpdate)
        {
            PlotArrows();
        }
    }

    [Button]
    private void PlotArrows()
    {
        SetUp();
        var ternaryPlot = GetComponent<TernaryPlot>();
        var container = ternaryPlot.GetContentContainer("VectorFieldArrows");

        var points = TernaryPlotUtility.EvenlyDistributedPoints(automaticPointIncrements);
        foreach (var point in points)
        {
            var difference = TernaryDifferential(point);
            
            // Get the difference between start and end result as a normalized vector
            var pointAsVector = TernaryPlot.CoordinatesToPosition(point);
            var differenceVector = TernaryPlot.CoordinatesToPosition(difference);
            
            if (differenceVector.sqrMagnitude > 0.000001f)
            {
                differenceVector /= differenceVector.magnitude;
                var adjustedDifferenceVector = baseArrowLength * differenceVector / automaticPointIncrements;
                
                // Draw the arrow
                var arrow = container.AddArrow();
                arrow.thickness = baseArrowThickness / automaticPointIncrements;
                arrow.tail = pointAsVector - adjustedDifferenceVector / 2;
                arrow.head = pointAsVector + adjustedDifferenceVector / 2;
            }
        }

        container.Purge(defer: true);
    }

    protected abstract void SetUp();

    protected abstract float[] TernaryDifferential(float[] point);
}
