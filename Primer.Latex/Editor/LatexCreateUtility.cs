using Primer.Editor;
using UnityEditor;

namespace Primer.Latex.Editor
{
    public static class LatexCreateUtility
    {
        [MenuItem("GameObject/Primer/LaTeX", false, CreateUtility.PRIORITY)]
        public static void Latex() => CreateUtility.Prefab(LatexComponent.PREFAB_NAME);
    }
}
