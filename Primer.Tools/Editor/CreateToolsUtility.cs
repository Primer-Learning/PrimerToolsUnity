using Primer.Editor;
using UnityEditor;

namespace Primer.Tools.Editor
{
    public static class ToolsCreateUtility
    {
        [MenuItem("GameObject/Primer/Arrow", false, CreateUtility.PRIORITY)]
        public static void Graph() => CreateUtility.Prefab("Arrow");
    }
}
