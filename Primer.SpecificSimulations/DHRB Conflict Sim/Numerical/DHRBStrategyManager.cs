using System;
using System.Collections.Generic;
using Primer.Simulation;
using Simulation.GameTheory;
using UnityEngine;

[RequireComponent(typeof(DHRBRewardEditorComponent))]
public class DHRBStrategyManager : MonoBehaviour
{
    public DHRBRewardEditorComponent rewardEditor => GetComponent<DHRBRewardEditorComponent>();

    public List<DHRB> strategyList = new() {
        DHRB.Dove,
        DHRB.Hawk,
        DHRB.Retaliator,
    };

    // This method assumes the float array being given encodes a point on a plot.
    // It turns that point into an AlleleFrequency dictionary according to the 
    // Order of strategies included in strategyList.
    public AlleleFrequency<DHRB> AlleleFrequencyFromFloatArray(float[] values)
    {
        if (values.Length != strategyList.Count)
            throw new ArgumentException("StrategyManager received the wrong number of values.");
        
        // Form the AlleleFrequency, matching values to the appropriate strategy
        var alleleFrequency = new AlleleFrequency<DHRB>();
        foreach (var strategy in strategyList) {
            alleleFrequency[strategy] = values[strategyList.IndexOf(strategy)];
        }

        return alleleFrequency;
    }
    
    public float[] ToFloatArray(AlleleFrequency<DHRB> alleleFrequency)
    {
        var values = new float[strategyList.Count];
        for (var i = 0; i < values.Length; i++)
            values[i] = alleleFrequency[strategyList[i]];
        
        return values;
    }

}