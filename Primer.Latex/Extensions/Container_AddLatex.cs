namespace Primer.Latex
{
    public static class Container_AddLatexExtensions
    {
        public static LatexComponent AddLatex(this Container container, string formula, string name = null)
        {
            var child = container.AddPrefab<LatexComponent>(LatexComponent.PREFAB_NAME, name ?? formula);

            child.Process(formula);
            child.transform.SetScale(0);
            child.SetActive(true);

            return child;
        }
    }
}
