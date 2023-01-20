using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Axis
{
    public class AxisTick : MonoBehaviour
    {
        [HideInInspector]
        public PrimerText text;

        [ShowInInspector]
        public string label {
            get => text.text;
            set => text.text = value;
        }
    }
}
