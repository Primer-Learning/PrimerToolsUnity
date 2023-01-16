using Primer.Editor;
using UnityEditor;

namespace Primer.Table.Editor
{
    public static class TableCreateUtility
    {
        [MenuItem("GameObject/Primer/Table", false, CreateUtility.PRIORITY)]
        public static void Table() => CreateUtility.Prefab("Table");
    }
}
