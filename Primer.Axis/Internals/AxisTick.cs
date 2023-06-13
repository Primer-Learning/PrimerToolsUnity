using Primer.Latex;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Axis
{
    public class AxisTick : MonoBehaviour
    {
        public float value;

        private LatexComponent latexCache;
        private LatexComponent latex
            => latexCache != null ? latexCache : latexCache = GetComponentInChildren<LatexComponent>();

        [ShowInInspector]
        public string label {
            get => latex.latex;
            set => latex.latex = value;
        }
    }
}
