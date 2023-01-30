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
        private static readonly VectorUtils.TessellationOptions tessellationOptions = new() {
            StepDistance = 100.0f,
            MaxCordDeviation = 0.5f,
            MaxTanAngleDeviation = 0.1f,
            SamplingStepSize = 0.01f,
        };

        private record CreateSprites(
            List<VectorUtils.Geometry> Geometry,
            TaskCompletionSource<SvgSprite[]> Result,
            CancellationToken CancellationToken
        );

        [CanBeNull] private CreateSprites waitingToProcess;

        public async Task<SvgSprite[]> ConvertToSprites(string svg, CancellationToken ct)
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

            var taskCompletionSource = new TaskCompletionSource<SvgSprite[]>();

            waitingToProcess = new CreateSprites(
                VectorUtils.TessellateScene(sceneInfo.Scene, tessellationOptions),
                taskCompletionSource,
                ct
            );

            // See ProcessGeometry's <remarks> to know why
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
        ///     VectorUtils.BuildSprite (in LatexChar) has to be called on the Player update loop
        ///     we're already out of it since we waited for LaTeX to generate the SVG
        ///     so we need to force another update and hook to it
        ///     we can then use VectorUtils.BuildSprite without throwing
        ///     "Not allowed to override geometry on sprite"
        /// </remarks>
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

        private static SvgSprite[] ConvertGeometryToSprites(IReadOnlyCollection<VectorUtils.Geometry> allGeometry)
        {
            var allVertices = allGeometry.SelectMany(geometry => geometry.TransformVertices());
            var globalBounds = VectorUtils.Bounds(allVertices);

            var chars = allGeometry.Select(geometry => {
                var bounds = VectorUtils.Bounds(geometry.TransformVertices());
                var position = bounds.center - globalBounds.center;

                // This matches the way flipYAxis would work in BuildSprite if we gave it all of the geometry in the SVG
                // rather than just one at a time.
                position.y = -position.y;

                return new SvgSprite(bounds, position, geometry.ConvertToSprite());
            });

            return chars.ToArray();
        }
    }
}
