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
        public List<string> headers = LatexRenderConfig.GetDefaultHeaders();
        public Material material;


        #region Internal fields
        [NotNull] internal LatexChar[] characters = Array.Empty<LatexChar>();
        internal readonly LatexToSprites latexToSprites = new();
        internal readonly LatexDirectRenderer directRenderer = new();

        internal bool isValid => this != null && (Config.IsEmpty || characters.Length != 0 && characters.AreSpritesValid());
        internal LatexRenderConfig Config => new(latex, headers);
        #endregion


        // We mess with the update loop when rendering the sprites
        // so LateUpdate is required here
        private void LateUpdate()
        {
            if (!isValid) return;
            directRenderer.SetCharacters(characters, material);
            directRenderer.Draw(transform);
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
            internal LatexRenderConfig config => new(latex, headers);
        }


#if UNITY_EDITOR
        // This needs to be private (or internal) because SpriteDirectRenderer is internal
        [Tooltip("Which mesh features to visualize. Gizmos are only ever visible in the Unity editor.")]
        [SerializeField]
        private LatexDirectRenderer.GizmoMode gizmos = LatexDirectRenderer.GizmoMode.Nothing;


        #region Unity events
        private void OnDrawGizmos() => directRenderer.DrawWireGizmos(transform, gizmos);

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
            if (!isValid) {
                await Render(Config);
                LateUpdate();
            }
        }
        #endregion


        #region Proxy LatexToSprites
        public bool isRunning => latexToSprites.isRunning;
        public bool isCancelled => latexToSprites.isCancelled;
        public Exception renderError => latexToSprites.renderError;

        public void CancelRender() => latexToSprites.Cancel();
        public void OpenBuildDir() => latexToSprites.OpenBuildDir();

        public async Task Render(LatexRenderConfig config)
        {
            characters = config.IsEmpty
                ? Array.Empty<LatexChar>()
                : await latexToSprites.Render(config);

            latex = config.Latex;
            headers = config.Headers.ToList();
        }
        #endregion


        public Released ReleaseSvgParts()
        {
            var drawSpecs = directRenderer.drawSpecs;

            for (var i = 0; i < drawSpecs.Length; i++) {
                var drawSpec = drawSpecs[i];
                var obj = new GameObject($"SvgPart {i}");

                obj.AddComponent<MeshFilter>().sharedMesh = drawSpec.Mesh;
                obj.AddComponent<MeshRenderer>().material = material;

                obj.transform.SetParent(transform, false);
                obj.transform.localPosition = drawSpec.Position;

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
