using System;
using System.Collections.Generic;
using System.Linq;
using Primer.Animation;
using UnityEngine;

namespace Primer.Graph
{
    public class Graph3 : MonoBehaviour, IDisposable
    {
        public bool enableZAxis;
        public bool isRightHanded = true;

        public Axis3 xAxis => GameObject.Find("X").GetComponent<Axis3>(); 
        public Axis3 yAxis => GameObject.Find("Y").GetComponent<Axis3>(); 
        public Axis3 zAxis => GameObject.Find("Z").GetComponent<Axis3>();
        private List<Axis3> axes => new() { xAxis, yAxis, zAxis };
        public Tween Transition() => axes.Select(x => x.Transition()).RunInParallel();
        public Tween Appear() => axes.Select(x => x.Appear()).RunInParallel();
        public Tween Disappear() => axes.Select(x => x.Disappear()).RunInParallel();
        
        public void Dispose()
        {
            gameObject.SetActive(false);
        }
    }
}