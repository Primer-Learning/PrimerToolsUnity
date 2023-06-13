using Primer.Editor;
using UnityEditor;

namespace Primer.Graph.Editor
{
    public static class GraphCreateUtility
    {
        [MenuItem("GameObject/Primer/Graph", false, CreateUtility.PRIORITY)]
        public static void Graph() => CreateUtility.Prefab("Graph2");
    }
}
