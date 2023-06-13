using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Axis
{
    public class AxisTick : MonoBehaviour
    {
        public float value;

        [HideInInspector]
        public PrimerText2 text;

        [ShowInInspector]
        public string label {
            get => text.text;
            set => text.text = value;
        }
    }
}
