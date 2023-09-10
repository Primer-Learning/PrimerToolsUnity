using System;
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
        
        public void Dispose()
        {
            gameObject.SetActive(false);
        }
    }
}