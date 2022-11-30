using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Primer.Latex
{
    public class LatexToSprites
    {
        private readonly LatexToSvg latexToSvg = new();
        private readonly SvgToSprites svgToSprites = new();


        private Task<LatexChar[]> currentTask;
        [CanBeNull] private CancellationTokenSource cancellationSource;


        internal bool isRunning => currentTask?.IsCompleted == false;
        internal bool isCancelled => cancellationSource?.IsCancellationRequested == true;
        internal Exception renderError => currentTask?.Exception;

        public void Cancel() => cancellationSource?.Cancel();

        public void OpenBuildDir() => latexToSvg.rootTempDir.Open();


        public Task<LatexChar[]> Render(LatexRenderConfig config)
        {
            cancellationSource = new CancellationTokenSource();
            currentTask = Render(config, cancellationSource.Token);
            return currentTask;
        }

        private async Task<LatexChar[]> Render(LatexRenderConfig config, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(config.Latex))
                return new LatexChar[] {};

            ct.ThrowIfCancellationRequested();
            var svg = await latexToSvg.RenderToSvg(config, ct);
            ct.ThrowIfCancellationRequested();
            var renderedSprites = await svgToSprites.ConvertToSprites(svg, ct);
            ct.ThrowIfCancellationRequested();

            return renderedSprites;
        }


    }
}
