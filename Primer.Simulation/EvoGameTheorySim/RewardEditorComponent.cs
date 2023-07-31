using System;
using Primer.Simulation;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class RewardEditorComponent<T> : MonoBehaviour where T : Enum
{
    // Obviously there's a more general way to handle triggering/listening, but this is straightforward for now.
    [PropertyOrder(-2)]
    public TernaryVectorFieldPlotter vectorField;

    private void OnValidate()
    {
        if (vectorField is not null)
            vectorField.TriggeredPlotArrows();
    }
    
    public RewardMatrix<T> rewardMatrix => new(PutValuesInMatrix());

    public abstract float[,] PutValuesInMatrix();
}
