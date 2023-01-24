using System;
using Primer.Math;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Table
{
    [Serializable]
    [HideLabel]
    [InlineProperty]
    [DisableContextMenu]
    [HideReferenceObjectPicker]
    public class CellPlacerEquation : ParametricEquation
    {
        public Vector3 start = Vector3.zero;
        public Vector3 end = new(10, 10, 10);


        public override Vector3 Evaluate(float t, float u)
        {
            var diff = end - start;

            return new Vector3(
                t * diff.x + start.x,
                u * diff.y + start.y,
                0 * diff.z + start.z
            );
        }
    }
}
