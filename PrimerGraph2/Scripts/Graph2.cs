using UnityEngine;
using UnityEngine.Serialization;

/* TODO
 * (Core)
 * Add precision options for tic labels
 *
 * (When needed)
 * Make padding amount uniform instead of using fraction of each axis length (maybe)
 * Destroy unused tics, animating them to zero scale
 */



[ExecuteInEditMode]
public class Graph2 : PrimerBehaviour
{
    [FormerlySerializedAs("ticLabelDistanceVertical")]
    [Header("Other")]
    public float ticLabelDistance = 0.25f;
    // public float ticLabelDistanceHorizontal = 0.65f;
    [Range(0, 0.5f)]
    public float paddingFraction = 0.05f;
    public bool rightHanded = true;

    public PrimerBehaviour arrowPrefab;
    public PrimerText2 primerTextPrefab;
    public Tic2 ticPrefab;

    public void Regenerate() {
        foreach (var axis in GetComponentsInChildren<Axis2>()) {
            axis.UpdateChildren();
        }
    }
}
