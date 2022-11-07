using UnityEditor;

public static class GraphCreateUtility
{
    [MenuItem("GameObject/Primer/Graph")]
    public static void Graph() => CreateUtility.Prefab("Graph2");

    // [MenuItem("GameObject/Primer/GraphCurve")]
    // public static void Curve() => CreateUtility.Prefab("GraphLine");
}
