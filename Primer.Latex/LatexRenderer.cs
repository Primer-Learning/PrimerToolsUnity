using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.Events;

namespace Primer.Latex
{
    [ExecuteAlways]
    [AddComponentMenu("Primer/Latex Renderer")]
    public class LatexRenderer : MonoBehaviour
    {
        [SerializeField][TextArea]
        private string latex = "";
        [SerializeField]
        [Tooltip(@"These will be inserted into the LaTeX template before \begin{document}.")]
        private List<string> headers = LatexInput.GetDefaultHeaders();
        public Material material;

        public LatexInput Config => new(latex, headers);
        public UnityEvent<LatexChar[]> onChange = new();


        #region Internal fields
        internal readonly LatexProcessor processor = LatexProcessor.GetInstance();
        [NotNull] internal LatexChar[] characters = Array.Empty<LatexChar>();

        internal bool isValid => characters.Length > 0 && characters.All(x => x.isSpriteValid);
        internal bool hasContent => !Config.IsEmpty || characters.Length > 0;
        #endregion


        #region Unity events
        private async void OnEnable()
        {
            if (hasContent && !isValid) {
                await Process(Config);

                // the component is destroyed after calling OnEnabled() when starting play mode
                if (this != null)
                    LateUpdate();
            }
        }

        // We mess with the update loop when rendering the sprites
        // so LateUpdate is required here
        private void LateUpdate()
        {
            if (hasContent && isValid)
                Render();
        }
        #endregion


        public async Task Process(LatexInput input)
        {
            var prevCharacters = characters;
            characters = await processor.Process(input);

            if (!prevCharacters.SequenceEqual(characters))
                onChange.Invoke(characters);
        }

        private void Render()
        {
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
