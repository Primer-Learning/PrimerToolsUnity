using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.VectorGraphics;
using UnityEditor;
using UnityEngine;

namespace Primer.Latex
{
    internal class SvgToSprites
    {
        const float SVG_PIXELS_PER_UNIT = 10f;

        /// <summary>Represents a single request to build an SVG.</summary>
        /// <remarks>
        ///     <para>Used to pass an SVG into the player loop for BuildSprites() to build.</para>
        ///     <para>buildSpritesResult will always return null if successful.</para>
        /// </remarks>
        (TaskCompletionSource<LatexAsSprites> result, string svg)? svgToBuildSpritesFor;


        public async Task<LatexAsSprites> ConvertToSprites(string svg, CancellationToken ct) {
            var completionSource = new TaskCompletionSource<LatexAsSprites>();

            ct.ThrowIfCancellationRequested();
            svgToBuildSpritesFor = (completionSource, svg);

#if UNITY_EDITOR
            // Update normally gets called only sporadically in the editor
            if (!Application.isPlaying)
                EditorApplication.QueuePlayerLoopUpdate();
#endif

            var result = await completionSource.Task;
            ct.ThrowIfCancellationRequested();
            return result;
        }


        public void OnPlayerLoop(MonoBehaviour renderer) {
            if (!svgToBuildSpritesFor.HasValue) return;
            var (result, svg) = svgToBuildSpritesFor.Value;

            try {
                var spritesWithPosition = BuildSprites(svg);

#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.RecordObject(renderer, "Set LaTeX");
#endif

                var sprites = spritesWithPosition.Select(i => i.Item2).ToArray();
                var positions = spritesWithPosition.Select(i => (Vector3)i.Item1).ToArray();
                result.SetResult(new LatexAsSprites(sprites, positions));
            }
            catch (Exception err) {
                result.SetException(err);
            }
            finally {
                svgToBuildSpritesFor = null;
            }
        }


        /// <remarks>Must be run inside the player loop.</remarks>
        static List<(Vector2 position, Sprite sprite)> BuildSprites(string svg) {
            SVGParser.SceneInfo sceneInfo;

            try {
                sceneInfo = SVGParser.ImportSVG(new StringReader(svg));
            }
            catch (Exception e) {
                Debug.LogError($"Invalid SVG, got error: {e}");
                return null;
            }

            var tessellationOptions = new VectorUtils.TessellationOptions {
                StepDistance = 100.0f,
                MaxCordDeviation = 0.5f,
                MaxTanAngleDeviation = 0.1f,
                SamplingStepSize = 0.01f
            };

            var allGeometry = VectorUtils.TessellateScene(sceneInfo.Scene, tessellationOptions);
            var scaledBounds = VectorUtils.Bounds(
                from geometry in allGeometry
                from vertex in geometry.Vertices
                select geometry.WorldTransform * vertex / SVG_PIXELS_PER_UNIT
            );

            // Holds an (offset, sprite) for each shape in the SVG
            var sprites = new List<(Vector2, Sprite)>(allGeometry.Count);

            foreach (var geometry in allGeometry) {
                var offset = VectorUtils.Bounds(
                    from vertex in geometry.Vertices
                    select geometry.WorldTransform * vertex / SVG_PIXELS_PER_UNIT
                ).center;

                offset -= scaledBounds.center;

                // This matches the way flipYAxis would work in BuildSprite if we gave it all of the geometry in the SVG
                // rather than just one at a time.
                offset.y = -offset.y;

                var buildSprite = VectorUtils.BuildSprite(
                    new List<VectorUtils.Geometry> {geometry},
                    SVG_PIXELS_PER_UNIT,
                    VectorUtils.Alignment.Center,
                    Vector2.zero,
                    128,
                    true
                );

                sprites.Add((offset, buildSprite));
            }

            return sprites;
        }
    }
}
