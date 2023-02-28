using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Primer.Latex.Editor
{
    [CustomEditor(typeof(LatexComponent))]
    public class LatexComponentEditor : OdinEditor
    {
        // This ensures the InfoBox gets update as the program processes the LaTeX
        public override bool RequiresConstantRepaint() => true;
    }
}
