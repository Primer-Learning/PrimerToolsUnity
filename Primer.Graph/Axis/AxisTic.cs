using Primer.Latex;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    public class AxisTic : MonoBehaviour
    {
        public float value;

        private LatexComponent latexCache;
        public LatexComponent latex => transform.ChildComponent(ref latexCache);

        [ShowInInspector]
        public string label {
            get => latex.latex;
            set => latex.latex = value;
        }
    }
}
