using System;
using System.Collections.Generic;
using Primer.Animation;
using UnityEngine;

namespace Primer.Graph
{
    public class StackedArea : MonoBehaviour
    {
        public PrimerLine this[int index] {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public void SetData(params IEnumerable<float>[] data)
        {
            throw new NotImplementedException();
        }

        public void AddData(float i, float i1)
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
