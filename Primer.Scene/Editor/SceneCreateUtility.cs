using Primer.Editor;
using UnityEditor;

namespace Primer.Scene.Editor
{
    public static class SceneCreateUtility
    {
        [MenuItem("GameObject/Primer/Camera", false, CreateUtility.PRIORITY)]
        public static void Graph() => CreateUtility.Prefab("PrimerCamera");
    }
}
