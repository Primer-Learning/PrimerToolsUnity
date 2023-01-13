using System;
using Primer.Math;
using Primer.Timeline;
using UnityEngine;

namespace Primer.Grid
{
    [Serializable]
    public class CellDisplacerBehaviour : PrimerPlayable
    {
        internal ParametricEquation equation;
        internal bool multiplyExistingPosition;

        public Vector3 Evaluate(Vector3 position, float tx, float ty)
        {
            var value = equation.Evaluate(tx, ty);
            return multiplyExistingPosition ? Vector3.Scale(position, value) : value;
        }
    }
}
