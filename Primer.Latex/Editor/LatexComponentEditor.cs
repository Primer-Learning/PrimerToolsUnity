using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Primer.Latex.Editor
{
    [CustomEditor(typeof(LatexComponent))]
    public class LatexComponentEditor : OdinEditor
    {
        private LatexComponent component => target as LatexComponent;

        // This ensures the InfoBox gets update as the program processes the LaTeX
        public override bool RequiresConstantRepaint() => true;

        public override void OnInspectorGUI()
        {
            // if (component.gameObject.IsPreset()) {
            //     ShowPresetWarning();
            //     return;
            // }

            base.OnInspectorGUI();
            CacheManagement();
        }

        private void ShowPresetWarning()
        {
            const string message = "You are editing a preset and the LaTeX will not be built until "
                + "you apply the preset to an actual LatexRenderer component.";

            EditorGUILayout.HelpBox(message, MessageType.Warning);
        }

        private void CacheManagement()
        {
            using var scope = new GUILayout.HorizontalScope();

            if (GUILayout.Button("Clear cache")) {
                LatexProcessingCache.ClearCache();
                component.Process(component.config).Forget();
            }

            if (GUILayout.Button("Open cache dir")) {
                LatexProcessingCache.OpenCacheDir();
            }

            LatexProcessingCache.disableCache = GUILayout.Toggle(
                LatexProcessingCache.disableCache,
                "Disable cache"
            );
        }
    }
}
