using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Primer.Latex
{
    internal class CancellableLatexProcessor : ProcessingPipeline
    {
        private Task<LatexChar[]> currentTask;
        private Task<LatexChar[]> lastTask;
        [CanBeNull] private CancellationTokenSource cancellationSource;

        public CancellableLatexProcessor(ILatexProcessor innerProcessor) : base(innerProcessor) {}

#if UNITY_EDITOR
        internal Exception renderError;

        public void Cancel() => cancellationSource?.Cancel();
#endif

        public override async Task<LatexChar[]> Process(LatexInput config, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var isOver = false;

            using var current = new CancellationTokenSource();
            cancellationSource = current;

            cancellationToken.Register(() => {
                // We know this variable is modified outside, that's why we are checking it
                // to know if the end of the outer function has been reached
                // thus, we disable ReSharper's validations
                //
                // ReSharper disable once AccessToModifiedClosure
                if (!isOver) {
                    // ReSharper disable once AccessToDisposedClosure
                    current.Cancel();
                }
            });

            var result = await processor.Process(config, cancellationSource.Token);

            isOver = true;
            cancellationSource = null;

            return result;
        }
    }
}
