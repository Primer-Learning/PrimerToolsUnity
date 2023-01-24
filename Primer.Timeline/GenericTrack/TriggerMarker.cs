using System;
using UnityEngine;

namespace Primer.Timeline
{
    [Serializable]
    public class TriggerMarker : PrimerMarker
    {
        [HideInInspector]
        [SerializeField]
        public string method;
    }
}
