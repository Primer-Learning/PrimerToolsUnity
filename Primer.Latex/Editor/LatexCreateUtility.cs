using Primer.Editor;
using UnityEditor;

namespace Primer.Latex.Editor
{
    public class LatexCreateUtility
    {
        [MenuItem("GameObject/Primer/LaTeX", false, CreateUtility.PRIORITY)]
        public static void Latex() => CreateUtility.Prefab("LaTeX");
    }
}
