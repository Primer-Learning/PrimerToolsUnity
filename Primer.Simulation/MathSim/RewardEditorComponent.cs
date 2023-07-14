using System;
using Primer.Simulation;
using UnityEngine;

public abstract class RewardEditorComponent<T> : MonoBehaviour where T : Enum
{
    // Obviously there's a more general way to handle triggering/listening, but this is straightforward for now.
    public TernaryVectorFieldPlotter vectorField;
    private void OnValidate()
    {
        if (vectorField is not null)
            vectorField.TriggeredPlotArrows();
    }
    
    public RewardMatrix<T> rewardMatrix {
        get
        {
            return new RewardMatrix<T>(PutValuesInMatrix());
        }
    }

    public abstract float[,] PutValuesInMatrix();
}
