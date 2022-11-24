using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Primer.Latex
{
    [ExecuteInEditMode]
    [AddComponentMenu("Primer/Latex Renderer")]
    public class LatexRenderer : MonoBehaviour
    {
        #region Configuration
        [TextArea]
        public string latex = "";

        [Tooltip(@"These will be inserted into the LaTeX template before \begin{document}.")]
        public List<string> headers = new()
        {
            @"\documentclass[preview]{standalone}",
            @"\usepackage[english]{babel}",
            @"\usepackage[utf8]{inputenc}",
            @"\usepackage[T1]{fontenc}",
            @"\usepackage{amsmath}",
            @"\usepackage{amssymb}",
            @"\usepackage{dsfont}",
            @"\usepackage{setspace}",
            @"\usepackage{tipa}",
            @"\usepackage{relsize}",
            @"\usepackage{textcomp}",
            @"\usepackage{mathrsfs}",
            @"\usepackage{calligra}",
            @"\usepackage{wasysym}",
            @"\usepackage{ragged2e}",
            @"\usepackage{physics}",
            @"\usepackage{xcolor}",
            @"\usepackage{microtype}",
            @"\usepackage{pifont}",
            @"\linespread{1}"
        };

        public Material material;
        #endregion


        internal readonly TempDir rootBuildDirectory;
        internal readonly LatexToSvg latexToSvg;
        internal readonly SvgToSprites svgToSprites = new();
        internal readonly SpriteDirectRenderer spritesRenderer = new();

        public LatexRenderer() {
            rootBuildDirectory = new TempDir();
            latexToSvg = new LatexToSvg(rootBuildDirectory);
        }


        public string Latex => AreSpritesValid ? latex : null;
        public IReadOnlyList<string> Headers => AreSpritesValid ? headers : null;
        public LatexRenderConfig Config => new(Latex, Headers);


        #region Sprites
        public Vector3[] spritesPositions;
        public Sprite[] sprites;

        /// <summary>
        ///     This is expected to happen when a LatexRenderer is
        ///         (a) initialized from defaults,
        ///         (b) set via a preset in the editor
        ///         (c) and occasionally when undoing/redoing.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The sprite assets are added as subassets of the scene the LatexRenderer is in, and can be
        ///         garbage collected any time if there's no LatexRenderer referencing them. This means when
        ///         redoing/undoing you can arrive in a state where the LatexRenderer is pointing at sprites
        ///         that have been cleaned up. Fortunately Unity notices when deserializing and the sprites
        ///         appear as null values in our list.
        ///     </para>
        ///     <para>
        ///         Presets that refer to a Sprite also don't prevent the sprite from being garbage collected
        ///         (and the preset could be applied to a LatexRenderer in a different scene anyways). So
        ///         presets often cause this same issue. Finally, when editing a preset directly, we actually
        ///         set its stored _sprites value to null directly since we need to make sure we never have a
        ///         mismatch between Latex & Headers text and the stored sprites.
        ///     </para>
        ///     <para>
        ///         I suspect there's a way to handle these situations using hooks into various parts of the
        ///         editor. But a decently thorough dive into the options had me arrive at the conclusion that
        ///         the approach here (just recognizing the mismatch and having the inspector rebuild) is the
        ///         simplest approach. Hopefully as my domain knowledge of the editor improves I'll think of an
        ///         even cleaner way though.
        ///     </para>
        /// </remarks>
        public bool AreSpritesValid =>
            // If the Sprite has been garbage collected, it will not be exactly null but will instead
            // be a special "null unity object". `value == null` or `!value` need to be used to check it.
            sprites is not null && sprites.All(i => (bool)i);
        #endregion


        public async Task RenderLatex(LatexRenderConfig config, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            var svg = await latexToSvg.RenderToSvg(config, ct);
            var renderedSprites = await svgToSprites.ConvertToSprites(svg, ct);

            ct.ThrowIfCancellationRequested();

            latex = config.Latex;
            headers = config.Headers.ToList();
            sprites = renderedSprites.Sprites;
            spritesPositions = renderedSprites.Positions;
        }

        // We mess with the update loop when rendering the sprites
        // so LateUpdate is required here
        void LateUpdate() {
            if (AreSpritesValid && spritesPositions is not null) {
                spritesRenderer.SetSprites(sprites, spritesPositions, material);
                spritesRenderer.Draw(transform);
            }
        }


#if UNITY_EDITOR
        // This needs to be private (or internal) because SpriteDirectRenderer is internal
        [Tooltip("Which mesh features to visualize. Gizmos are only ever visible in the Unity editor.")]
        [SerializeField]
        SpriteDirectRenderer.GizmoMode gizmos = SpriteDirectRenderer.GizmoMode.Nothing;

        void OnDrawGizmos() => spritesRenderer.DrawWireGizmos(transform, gizmos);

        void Reset() {
            // A default preset will automatically get applied when we're reset.
            // If we unconditionally set material here, we'll blow away the value it set.
            var presets = Preset.GetDefaultPresetsForObject(this);

            if (presets.All(preset => preset.excludedProperties.Contains("material"))) {
                material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            }
        }
#endif
    }
}
