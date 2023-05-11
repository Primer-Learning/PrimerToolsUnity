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

        public static Transform NextLatex(
            this ChildrenDeclaration children,
            ref Transform cache,
            string formula)
        {
            return children.UseCache(cache) ?? (cache = children.NextLatex(LatexComponent.PREFAB_NAME, formula).transform);
        }

        public static Transform NextLatex(
            this ChildrenDeclaration children,
            ref Transform cache,
            string name,
            string formula)
        {
            return children.UseCache(cache) ?? (cache = children.NextLatex(name, formula).transform);
        }

        public static TCached NextLatex<TCached>(
            this ChildrenDeclaration children,
            ref TCached cache,
            string name,
            string formula,
            Func<LatexComponent, TCached> init)
            where TCached : Component
        {
            return children.UseCache(cache) ?? (cache = children.NextLatex(name, formula, init));
        }

        public static TCached NextLatex<TCached>(
            this ChildrenDeclaration children,
            string formula,
            Func<LatexComponent, TCached> init)
            where TCached : Component
        {
            var child = children.NextLatex(LatexComponent.PREFAB_NAME, formula);
            return init(child);
        }

        public static TCached NextLatex<TCached>(
            this ChildrenDeclaration children,
            string name,
            string formula,
            Func<LatexComponent, TCached> init)
            where TCached : Component
        {
            var child = children.NextLatex(name, formula);
            return init(child);
        }
    }
}
