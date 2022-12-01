using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Primer.Latex
{
    public class LatexProcessor
    {
        private readonly LatexToSvg latexToSvg = new();
        private readonly SvgToChars svgToChars = new();


        private Task<LatexChar[]> currentTask;
        [CanBeNull] private CancellationTokenSource cancellationSource;


#if UNITY_EDITOR
        internal bool isRunning => currentTask?.IsCompleted == false;
        internal bool isCancelled => cancellationSource?.IsCancellationRequested == true;
        internal Exception renderError => currentTask?.Exception;

        public void Cancel() => cancellationSource?.Cancel();

        public void OpenBuildDir() => latexToSvg.rootTempDir.Open();
#endif

        public Task<LatexChar[]> Render(LatexInput config)
        {
            cancellationSource = new CancellationTokenSource();
            currentTask = Render(config, cancellationSource.Token);
            return currentTask;
        }

        private async Task<LatexChar[]> Render(LatexInput config, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(config.Latex))
                return new LatexChar[] {};

            ct.ThrowIfCancellationRequested();
            var svg = await latexToSvg.RenderToSvg(config, ct);
            ct.ThrowIfCancellationRequested();
            var renderedSprites = await svgToChars.ConvertToLatexChar(svg, ct);
            ct.ThrowIfCancellationRequested();

            return renderedSprites;
        }
    }
}
