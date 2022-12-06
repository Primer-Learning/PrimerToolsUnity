using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Primer.Latex
{
    [ExecuteAlways]
    [AddComponentMenu("Primer/Latex Renderer")]
    public class LatexRenderer : MonoBehaviour
    {
        [TextArea]
        public string latex = "";
        [Tooltip(@"These will be inserted into the LaTeX template before \begin{document}.")]
        public List<string> headers = LatexInput.GetDefaultHeaders();
        public Material material;


        #region Internal fields
        internal readonly LatexProcessor latexProcessor = new();
        [NotNull] internal LatexChar[] characters = Array.Empty<LatexChar>();

        internal bool isValid => characters.All(x => x.isSpriteValid);
        internal bool hasContent => Config.IsEmpty || characters.Length > 0;
        internal LatexInput Config => new(latex, headers);
        #endregion


        // We mess with the update loop when rendering the sprites
        // so LateUpdate is required here
        private void LateUpdate()
        {
            if (!hasContent || !isValid) return;

            for (var i = 0; i < characters.Length; i++) {
                characters[i].Draw(transform, material);
            }
        }


        // Using LatexRenderer as container like this prevents the ReleasedLatexRenderer from being created by the
        // editor user (ie: it won't appear in any menus or searches).
        public class Released : MonoBehaviour
        {
            [SerializeField] [HideInInspector]
            internal string latex;
            [SerializeField] [HideInInspector]
            internal List<string> headers;
            [SerializeField] [HideInInspector]
            internal Material material;
            internal LatexInput config => new(latex, headers);
        }


#if UNITY_EDITOR
        // This needs to be private (or internal) because SpriteDirectRenderer is internal
        [Tooltip("Which mesh features to visualize. Gizmos are only ever visible in the Unity editor.")]
        [SerializeField]
        private LatexChar.GizmoMode gizmos = LatexChar.GizmoMode.Nothing;


        #region Unity events
        private void OnDrawGizmos()
        {
            for (var i = 0; i < characters.Length; i++) {
                characters[i].DrawWireGizmos(transform, gizmos);
            }
        }

        private void Reset()
        {
            // A default preset will automatically get applied when we're reset.
            // If we unconditionally set material here, we'll blow away the value it set.
            var presets = Preset.GetDefaultPresetsForObject(this);

            if (presets.All(preset => preset.excludedProperties.Contains("material"))) {
                material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            }
        }

        private async void OnEnable()
        {
            if (hasContent && !isValid) {
                await Render(Config);
                LateUpdate();
            }
        }
        #endregion


        #region Proxy LatexToSprites
        public bool isRunning => latexProcessor.isRunning;
        public bool isCancelled => latexProcessor.isCancelled;
        public Exception renderError => latexProcessor.renderError;

        public void CancelRender() => latexProcessor.Cancel();
        public void OpenBuildDir() => latexProcessor.OpenBuildDir();

        public async Task Render(LatexInput config)
        {
            characters = config.IsEmpty
                ? Array.Empty<LatexChar>()
                : await latexProcessor.Render(config);

            latex = config.Latex;
            headers = config.Headers.ToList();
        }
        #endregion


        public Released ReleaseSvgParts()
        {
            for (var i = 0; i < characters.Length; i++) {
                var character = characters[i];
                var obj = new GameObject($"SvgPart {i}");

                obj.AddComponent<MeshFilter>().sharedMesh = character.mesh;
                obj.AddComponent<MeshRenderer>().material = material;

                obj.transform.SetParent(transform, false);
                obj.transform.localPosition = character.position;

                Undo.RegisterCreatedObjectUndo(obj, "");
            }

            var releasedRenderer = gameObject.AddComponent<Released>();
            releasedRenderer.latex = latex;
            releasedRenderer.headers = headers;
            releasedRenderer.material = material;
            return releasedRenderer;
        }
#endif
    }
}