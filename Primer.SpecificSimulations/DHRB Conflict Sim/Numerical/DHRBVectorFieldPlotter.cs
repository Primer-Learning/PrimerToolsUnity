using Primer.Graph;
using Primer.Simulation;
using Simulation.GameTheory;
using UnityEngine;

[RequireComponent(typeof(TernaryPlot))]
public class DHRBVectorFieldPlotter : TernaryVectorFieldPlotter
{
    public DHRBNumericalSimStrategyManager NumericalSimStrategyManager => FindObjectOfType<DHRBNumericalSimStrategyManager>();
    private NumericalEvoGameTheorySim<DHRB> sim;

    protected override void SetUp()
    {
        if (NumericalSimStrategyManager.strategyList.Count == 4) ternaryPlot.isQuaternary = true;
        else if (NumericalSimStrategyManager.strategyList.Count == 3) ternaryPlot.isQuaternary = false;
        else Debug.LogError($"The sim runner expects 3 or 4 strategies, but there are {NumericalSimStrategyManager.strategyList.Count}");
        sim = new NumericalEvoGameTheorySim<DHRB>(NumericalSimStrategyManager.rewardEditor.rewardMatrix, NumericalSimStrategyManager.rewardEditor.baseFitness, 0);
    }
    
    protected override float[] TernaryDifferential(float[] point)
    {
        // Figure out what the result would be for a single step of the sim.
        var pointAsAlleleFrequency = NumericalSimStrategyManager.AlleleFrequencyFromFloatArray(point);
        var result = sim.SingleIteration(pointAsAlleleFrequency, stepSize: 1);
        
        return TernaryPlotUtility.SubtractFloatArrays(NumericalSimStrategyManager.ToFloatArray(result), point);
    }
}
