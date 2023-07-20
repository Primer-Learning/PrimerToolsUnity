using System;
using System.Collections.Generic;
using Primer.Animation;
using UnityEngine;

namespace Primer.Graph
{
    public class PrimerLine : MonoBehaviour
    {
        public void SetData(IEnumerable<float> data)
        {
            throw new NotImplementedException();
        }
        public void SetData(IEnumerable<Vector2> data)
        {
            throw new NotImplementedException();
        }
        public void SetData(IEnumerable<Vector3> data)
        {
            throw new NotImplementedException();
        }

        public void AddPoint(float data)
        {
            throw new NotImplementedException();
        }
        public void AddPoint(Vector2 data)
        {
            throw new NotImplementedException();
        }
        public void AddPoint(Vector3 data)
        {
            throw new NotImplementedException();
        }

        // resolution default is last set resolution or 10
        public void SetFunction(Func<float, float> function, int? resolution = null)
        {
            throw new NotImplementedException();
        }
        public void SetFunction(Func<float, Vector2> function, int? resolution = null)
        {
            throw new NotImplementedException();
        }
        public void SetFunction(Func<float, Vector3> function, int? resolution = null)
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
