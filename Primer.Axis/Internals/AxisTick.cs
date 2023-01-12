using UnityEngine;

namespace Primer.Axis
{
    public class AxisTick : MonoBehaviour
    {
        public PrimerText2 text;

        public string label {
            get => text.text;
            set => text.text = value;
        }
    }
}
