using UnityEditor;

namespace Primer.Editor
{
    public static class PrimerCreateUtility
    {
        [MenuItem("GameObject/Primer/Label", false, CreateUtility.PRIORITY)]
        public static void Label() => CreateUtility.Prefab("PrimerLabel");
    }
}
