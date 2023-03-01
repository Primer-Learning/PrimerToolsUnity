using Primer.Editor;
using UnityEditor;

namespace Primer.Tools.Editor
{
    public static class ToolsCreateUtility
    {
        [MenuItem("GameObject/Primer/Arrow", false, CreateUtility.PRIORITY)]
        public static void Arrow() => CreateUtility.Prefab("Arrow");

        [MenuItem("GameObject/Primer/Bracket", false, CreateUtility.PRIORITY)]
        public static void Bracket() => CreateUtility.Prefab("Bracket2");
    }
}
