using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Latex
{
    // Using a container like this prevents the ReleasedLatexRenderer from being created by the
    // editor user (ie: it won't appear in any menus or searches).
    public static class ReleasedLatexRendererContainer
    {
        public class ReleasedLatexRenderer : MonoBehaviour
        {
            [SerializeField] [HideInInspector] string latex;
            [SerializeField] [HideInInspector] List<string> headers;
            [SerializeField] [HideInInspector] Material material;

            public Material Material => material;
            public IReadOnlyList<string> Headers => headers;
            public string Latex => latex;
            public LatexRenderConfig Config => new(latex, headers);

            internal void SetLatex(LatexRenderConfig config, Material material) {
                latex = config.Latex;
                headers = config.Headers.ToList();
                this.material = material;
            }
        }
    }
}
