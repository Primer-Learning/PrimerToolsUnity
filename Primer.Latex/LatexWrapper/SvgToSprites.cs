using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Unity.VectorGraphics;
using UnityEditor;
using UnityEngine;

namespace Primer.Latex
{
    internal class SvgToSprites
    {
        const float SVG_PIXELS_PER_UNIT = 10f;
        static readonly VectorUtils.TessellationOptions tessellationOptions = new() {
            StepDistance = 100.0f,
            MaxCordDeviation = 0.5f,
            MaxTanAngleDeviation = 0.1f,
            SamplingStepSize = 0.01f
        };

        record CreateSprites(
            List<VectorUtils.Geometry> Geometry,
            TaskCompletionSource<LatexChar[]> Result,
            CancellationToken CancellationToken
        );

        [CanBeNull] CreateSprites waitingToProcess;

        public async Task<LatexChar[]> ConvertToSprites(string svg, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            SVGParser.SceneInfo sceneInfo;

            try {
                sceneInfo = SVGParser.ImportSVG(new StringReader(svg));
            }
            catch (Exception e) {
                Debug.LogError($"Invalid SVG, got error: {e}");
                return null;
            }

            var taskCompletionSource = new TaskCompletionSource<LatexChar[]>();

            waitingToProcess = new CreateSprites(
                VectorUtils.TessellateScene(sceneInfo.Scene, tessellationOptions),
                taskCompletionSource,
                ct
            );

            // See ProcessGeometry docs to know why
            UnityEventHook.OnUpdate += ProcessGeometry;

#if UNITY_EDITOR
            // Update normally gets called only sporadically in the editor
            if (!Application.isPlaying)
                EditorApplication.QueuePlayerLoopUpdate();
#endif

            var result = await taskCompletionSource.Task;
            ct.ThrowIfCancellationRequested();
            return result;
        }

        /// <remarks>
        ///     VectorUtils.BuildSprite (in ConvertGeometryToSprites) has to be called on the Player update loop
        ///     we're already out of it since we waited for LaTeX to generate the SVG
        ///     so we need to force another update and hook to it
        ///     we can then use VectorUtils.BuildSprite without throwing
        ///     "Not allowed to override geometry on sprite"
        /// <remarks>
        private void ProcessGeometry()
        {
            UnityEventHook.OnUpdate -= ProcessGeometry;
            if (waitingToProcess is null) return;

            var allGeometry = waitingToProcess.Geometry;
            var result = waitingToProcess.Result;
            var ct = waitingToProcess.CancellationToken;

            waitingToProcess = null;

            if (ct.IsCancellationRequested) {
                result.SetCanceled();
                return;
            }

            try {
                ct.ThrowIfCancellationRequested();
                result.SetResult(ConvertGeometryToSprites(allGeometry));
            }
            catch (Exception ex) {
                result.SetException(ex);
            }
        }

        static LatexChar[] ConvertGeometryToSprites(List<VectorUtils.Geometry> allGeometry)
        {
            var chars = new LatexChar[allGeometry.Count];

            var scaledBounds = VectorUtils.Bounds(
                from geometry in allGeometry
                from vertex in geometry.Vertices
                select geometry.WorldTransform * vertex / SVG_PIXELS_PER_UNIT
            );

            for (var i = 0; i < allGeometry.Count; i++) {
                var geometry = allGeometry[i];

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

                chars[i] = new LatexChar(buildSprite, offset);
            }

            return chars;
        }
    }
}
