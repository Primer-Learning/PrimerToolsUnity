using System;
using System.Collections.Generic;
using Primer.Animation;
using UnityEngine;

namespace Primer.Graph
{
    public class PrimerSurface : MonoBehaviour
    {
        // My mind breaks trying to define these signatures
        public void SetData(float[,] data)
        {
            throw new NotImplementedException();
        }
        public void SetData(IEnumerable<IEnumerable<float>> data)
        {
            throw new NotImplementedException();
        }
        // void SetData(IEnumerable<Vector3> data)

        // resolution default is last set resolution or 10
        public void SetFunction(Func<float, float, float> function, int? resolution = null)
        {
            throw new NotImplementedException();
        }
        public void SetFunction(Func<Vector2, Vector3> function, int? resolution = null)
        {
            throw new NotImplementedException();
        }

        public Tween Transition()
        {
            throw new NotImplementedException();
        }
        public Tween GrowFromStart()
        {
            throw new NotImplementedException();
        }
        public Tween ShrinkToEnd()
        {
            throw new NotImplementedException();
        }
    }
}
