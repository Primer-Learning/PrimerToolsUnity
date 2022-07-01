using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.VectorGraphics;
using UnityEditor;
using UnityEngine;

[assembly: InternalsVisibleTo("Primer.LatexRenderer.Editor")]

namespace LatexRenderer
{
    [ExecuteInEditMode]
    [SelectionBase]
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

        private readonly SpriteDirectRenderer _renderer = new();

        /// <summary>Represents a single request to build an SVG.</summary>
        /// <remarks>
        ///     <para>Used to pass an SVG into the player loop for BuildSprites() to build.</para>
        ///     <para>buildSpritesResult will always return null if successful.</para>
        /// </remarks>
        private (TaskCompletionSource<object> buildSpritesResult, string svg, string latex,
            List<string> headers)? _svgToBuildSpritesFor;

        public string Latex => _latex;
        public IReadOnlyList<string> Headers => _headers;


#if UNITY_EDITOR
        private void Reset()
        {
            material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        }
#endif

        public void Update()
        {
            if (_svgToBuildSpritesFor.HasValue)
                try
                {
                    var sprites = BuildSprites(_svgToBuildSpritesFor.Value.svg);

#if UNITY_EDITOR
                    if (Application.isEditor && !Application.isPlaying)
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

            if (_sprites is not null && _spritesPositions is not null)
            {
                _renderer.SetSprites(_sprites, _spritesPositions, material);
                _renderer.Draw(transform);
            }
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
                    select geometry.WorldTransform * vertex / SvgPixelsPerUnit).min;

                offset -= scaledBounds.center;

                // This matches the way flipYAxis would work in BuildSprite if we gave it all of the geometry in the SVG
                // rather than just one at a time.
                offset.y = -offset.y;

                sprites.Add((offset,
                    VectorUtils.BuildSprite(new List<VectorUtils.Geometry> { geometry },
                        SvgPixelsPerUnit, VectorUtils.Alignment.TopLeft, Vector2.zero, 128, true)));
            }

            return sprites;
        }

#if UNITY_EDITOR
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