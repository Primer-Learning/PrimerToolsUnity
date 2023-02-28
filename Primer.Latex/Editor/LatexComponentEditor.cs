using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Primer.Latex.Editor
{
    [CustomEditor(typeof(LatexComponent))]
    public class LatexComponentEditor : OdinEditor
    {
        private LatexComponent component => target as LatexComponent;

        public override bool RequiresConstantRepaint() => true;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!component.isActiveAndEnabled)
                component.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnEnable();

            if (!component.isActiveAndEnabled)
                component.OnDisable();
        }
    }
}
