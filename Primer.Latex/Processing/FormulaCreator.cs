using System;
using System.Threading;
using System.Threading.Tasks;

namespace Primer.Latex
{
    // TODO: Rename to ExpressionCreator
    internal class FormulaCreator : ILatexProcessor
    {
        private readonly LatexToSvg latexToSvg = new();
        private readonly SvgToChars svgToChars = new();

        public LatexProcessingState state { get; private set; } = LatexProcessingState.Initialized;

        public async Task<LatexExpression> Process(LatexInput config, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(config.code)) {
                state = LatexProcessingState.Completed;
                return new LatexExpression();
            }

            state = LatexProcessingState.Processing;

            try {
                var renderedSprites = await DelegateRendering(config, ct);
                state = LatexProcessingState.Completed;
                return new LatexExpression(renderedSprites);
            }
            catch (OperationCanceledException) {
                state = LatexProcessingState.Cancelled;
                throw;
            }
            catch (Exception) {
                state = LatexProcessingState.Errored;
                throw;
            }
        }

#if UNITY_EDITOR
        public void OpenBuildDir() => latexToSvg.rootTempDir.Open();
#endif

        private async Task<LatexChar[]> DelegateRendering(LatexInput config, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var svg = await latexToSvg.RenderToSvg(config, ct);

            ct.ThrowIfCancellationRequested();

            var renderedSprites = await svgToChars.ConvertToLatexChar(svg, ct);

            ct.ThrowIfCancellationRequested();

            return renderedSprites;
        }
    }
}
