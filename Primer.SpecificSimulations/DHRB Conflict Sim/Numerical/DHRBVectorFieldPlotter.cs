using Primer.Graph;
using Primer.Simulation;
using Simulation.GameTheory;
using UnityEngine;

[RequireComponent(typeof(TernaryPlot))]
public class DHRBVectorFieldPlotter : TernaryVectorFieldPlotter
{
    public DHRBStrategyManager strategyManager => FindObjectOfType<DHRBStrategyManager>();

    private NumericalEvoGameTheorySim<DHRB> sim;

    protected override void SetUp()
    {
        if (strategyManager.strategyList.Count == 4) ternaryPlot.isQuaternary = true;
        else if (strategyManager.strategyList.Count == 3) ternaryPlot.isQuaternary = false;
        else Debug.LogError($"The sim runner expects 3 or 4 strategies, but there are {strategyManager.strategyList.Count}");
        sim = new NumericalEvoGameTheorySim<DHRB>(strategyManager.rewardEditor.rewardMatrix, strategyManager.rewardEditor.baseFitness, 0);
    }
    
    protected override float[] TernaryDifferential(float[] point)
    {
        // Figure out what the result would be for a single step of the sim.
        var pointAsAlleleFrequency = strategyManager.AlleleFrequencyFromFloatArray(point);
        var result = sim.SingleIteration(pointAsAlleleFrequency, stepSize: 1);
        
        return TernaryPlotUtility.SubtractFloatArrays(strategyManager.ToFloatArray(result), point);
    }
}
