using System;
using UnityEngine;

namespace Primer.Latex
{
    public static class ChildrenDeclarationExtensions
    {
        private static LatexComponent prefabCache;
        private static LatexComponent prefab => prefabCache ??= Resources.Load<LatexComponent>(LatexComponent.PREFAB_NAME);

        public static LatexComponent NextLatex(this ChildrenDeclaration children, string formula, Action<LatexComponent> init = null)
        {
            var child = children.NextIsInstanceOf(prefab, LatexComponent.PREFAB_NAME, init);
            child.transform.SetDefaults();
            child.Process(formula);
            return child;
        }

        public static LatexComponent NextLatex(this ChildrenDeclaration children, string name, string formula, Action<LatexComponent> init = null)
        {
            var child = children.NextIsInstanceOf(prefab, name, init);
            child.transform.SetDefaults();
            child.Process(formula);
            return child;
        }
    }
}
