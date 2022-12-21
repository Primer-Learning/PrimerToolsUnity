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
        internal readonly CancellableLatexProcessor processor = new();
        [NotNull] internal LatexChar[] characters = Array.Empty<LatexChar>();

        internal LatexProcessingState state => processor.state;
        internal bool isValid => characters.Length > 0 && characters.All(x => x.isSpriteValid);
        internal bool hasContent => !Config.IsEmpty || characters.Length > 0;
        internal LatexInput Config => new(latex, headers);
        #endregion


        #region Unity events
        private async void OnEnable()
        {
            if (hasContent && !isValid) {
                await Render(Config);
                if (this != null) LateUpdate();
            }
        }

        // We mess with the update loop when rendering the sprites
        // so LateUpdate is required here
        private void LateUpdate()
        {
            if (!hasContent || !isValid) return;

            for (var i = 0; i < characters.Length; i++) {
                characters[i].Draw(transform, material);
            }
        }
        #endregion


        public async Task Render(LatexInput config)
        {
            (latex, headers) = config;
            characters = await processor.Render(config);
        }

        public void CancelRender() => processor.Cancel();


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


        #region Proxy LatexProcessor
        internal bool isRunning => processor.state == LatexProcessingState.Processing;
        internal bool isCancelled => processor.state == LatexProcessingState.Cancelled;

        public Exception renderError => processor.renderError;

        public void OpenBuildDir() => processor.OpenBuildDir();
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
