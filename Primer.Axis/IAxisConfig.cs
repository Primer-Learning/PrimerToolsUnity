using UnityEngine;

namespace Primer.Axis
{
    public interface IAxisConfig
    {
        PrimerText2 primerTextPrefab { get; }
        Transform arrowPrefab { get; }
        Tic2 ticPrefab { get; }
        float paddingFraction { get; }
        float ticLabelDistance { get; }
    }
}
