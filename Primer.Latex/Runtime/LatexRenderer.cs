using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.VectorGraphics;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

[assembly: InternalsVisibleTo("Primer.LatexRenderer.Editor")]

namespace LatexRenderer
{
    [ExecuteInEditMode]
    [AddComponentMenu("Primer Learning/Latex Renderer")]
    public class LatexRenderer : MonoBehaviour
    {
        private const float SvgPixelsPerUnit = 10f;

        [SerializeField] [TextArea] private string _latex = "";

        [Tooltip(@"These will be inserted into the LaTeX template before \begin{document}.")]
        [SerializeField]
        private List<string> _headers = new()
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

        [SerializeField] [HideInInspector] internal Vector3[] _spritesPositions;
        [SerializeField] [HideInInspector] internal Sprite[] _sprites;

        private readonly LatexToSvgConverter _converter = LatexToSvgConverter.Create();

        internal readonly SpriteDirectRenderer _renderer = new();

        /// <summary>Represents a single request to build an SVG.</summary>
        /// <remarks>
        ///     <para>Used to pass an SVG into the player loop for BuildSprites() to build.</para>
        ///     <para>buildSpritesResult will always return null if successful.</para>
        /// </remarks>
        private (TaskCompletionSource<object> buildSpritesResult, string svg, string latex,
            List<string> headers)? _svgToBuildSpritesFor;

        public string Latex
        {
            get
            {
                if (AreSpritesValid())
                    return _latex;

                return null;
            }
        }

        public IReadOnlyList<string> Headers
        {
            get
            {
                if (AreSpritesValid())
                    return _headers;

                return null;
            }
        }

        public void Update()
        {
            if (_svgToBuildSpritesFor.HasValue)
                try
                {
                    var sprites = BuildSprites(_svgToBuildSpritesFor.Value.svg);

#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        Undo.RecordObject(this, "Set LaTeX");
#endif

                    _sprites = sprites.Select(i => i.Item2).ToArray();
                    _spritesPositions = sprites.Select(i => (Vector3)i.Item1).ToArray();

                    _latex = _svgToBuildSpritesFor.Value.latex;
                    _headers = _svgToBuildSpritesFor.Value.headers;

                    _svgToBuildSpritesFor.Value.buildSpritesResult.SetResult(null);
                }
                catch (Exception err)
                {
                    _svgToBuildSpritesFor.Value.buildSpritesResult.SetException(err);
                }
                finally
                {
                    _svgToBuildSpritesFor = null;
                }

            if (AreSpritesValid() && _spritesPositions is not null)
            {
                _renderer.SetSprites(_sprites, _spritesPositions, material);
                _renderer.Draw(transform);
            }
        }

        /// <summary>
        ///     This is expected to happen when a LatexRenderer is (a) initialized from defaults, (b) set
        ///     via a preset in the editor, (c) and occasionally when undoing/redoing.
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
        internal bool AreSpritesValid()
        {
            // If the Sprite has been garbage collected, it will not be exactly null but will instead
            // be a special "null unity object". `value == null` or `!value` need to be used to check it.
            return _sprites is not null && _sprites.All(i => (bool)i);
        }

        public (CancellationTokenSource, Task) SetLatex(string latex, List<string> headers)
        {
            var (cancellationSource, task) = _converter.RenderLatexToSvg(latex, headers);
            return (cancellationSource, SetLatex(latex, headers, task));
        }

        private async Task SetLatex(string latex, List<string> headers, Task<string> renderTask)
        {
            var svg = await renderTask;

            var completionSource = new TaskCompletionSource<object>();
            _svgToBuildSpritesFor = (completionSource, svg, latex, new List<string>(headers));

#if UNITY_EDITOR
            // Update normally gets called only sporadically in the editor
            if (!Application.isPlaying)
                EditorApplication.QueuePlayerLoopUpdate();
#endif

            await completionSource.Task;
        }

        public DirectoryInfo GetRootBuildDirectory()
        {
            return _converter.TemporaryDirectoryRoot;
        }

        /// <remarks>Must be run inside the player loop.</remarks>
        private static List<(Vector2 position, Sprite sprite)> BuildSprites(string svgText)
        {
            SVGParser.SceneInfo sceneInfo;
            try
            {
                sceneInfo = SVGParser.ImportSVG(new StringReader(svgText));
            }
            catch (Exception e)
            {
                Debug.LogError($"Invalid SVG, got error: {e}");
                return null;
            }

            var allGeometry = VectorUtils.TessellateScene(sceneInfo.Scene,
                new VectorUtils.TessellationOptions
                {
                    StepDistance = 100.0f,
                    MaxCordDeviation = 0.5f,
                    MaxTanAngleDeviation = 0.1f,
                    SamplingStepSize = 0.01f
                });

            var scaledBounds = VectorUtils.Bounds(from geometry in allGeometry
                from vertex in geometry.Vertices
                select geometry.WorldTransform * vertex / SvgPixelsPerUnit);

            // Holds an (offset, sprite) for each shape in the SVG
            var sprites = new List<(Vector2, Sprite)>(allGeometry.Count);

            foreach (var geometry in allGeometry)
            {
                var offset = VectorUtils.Bounds(from vertex in geometry.Vertices
                    select geometry.WorldTransform * vertex / SvgPixelsPerUnit).center;

                offset -= scaledBounds.center;

                // This matches the way flipYAxis would work in BuildSprite if we gave it all of the geometry in the SVG
                // rather than just one at a time.
                offset.y = -offset.y;

                sprites.Add((offset,
                    VectorUtils.BuildSprite(new List<VectorUtils.Geometry> { geometry },
                        SvgPixelsPerUnit, VectorUtils.Alignment.Center, Vector2.zero, 128, true)));
            }

            return sprites;
        }


#if UNITY_EDITOR
        private void Reset()
        {
            // A default preset will automatically get applied when we're reset. If we unconditionally
            // set material here, we'll blow away the value it set.
            var presets = Preset.GetDefaultPresetsForObject(this);
            if (presets.Length == 0 ||
                presets.All(preset => preset.excludedProperties.Contains("material")))
                material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        }

        // This needs to be private (or internal) because SpriteDirectRenderer is internal
        [Tooltip(
            "Which mesh features to visualize. Gizmos are only ever visible in the Unity editor.")]
        [SerializeField]
        private SpriteDirectRenderer.GizmoMode gizmos = SpriteDirectRenderer.GizmoMode.Nothing;

        private void OnDrawGizmos()
        {
            _renderer.DrawWireGizmos(transform, gizmos);
        }
#endif
    }
}