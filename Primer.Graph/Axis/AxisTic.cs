using Primer.Latex;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    public class AxisTic : MonoBehaviour
    {
        public float value;
        
        public LatexComponent latex => transform.Find("Label").GetOrAddComponent<LatexComponent>();

        [ShowInInspector]
        public string label {
            get => latex.latex;
            set => latex.latex = value;
        }
    }
}
