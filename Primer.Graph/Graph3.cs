using System;
using UnityEngine;

namespace Primer.Graph
{
    public class Graph3 : MonoBehaviour, IDisposable
    {
        public void Dispose()
        {
            gameObject.SetActive(false);
        }
    }
}