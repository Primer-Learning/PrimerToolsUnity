using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Primer.Latex
{
    public class CancellableLatexProcessor : ILatexProcessor
    {
        private readonly ILatexProcessor processor = CreateProcessor();

        private Task<LatexChar[]> currentTask;
        private Task<LatexChar[]> lastTask;
        [CanBeNull] private CancellationTokenSource cancellationSource;

        public LatexProcessingState state => processor.state;

#if UNITY_EDITOR
        internal Exception renderError;

        public void Cancel() => cancellationSource?.Cancel();

        public void OpenBuildDir() => processor.OpenBuildDir();
#endif

        public async Task<LatexChar[]> Render(LatexInput config, CancellationToken cancellationToken = default)
        {
            cancellationSource = new CancellationTokenSource();
            renderError = null;

            try {
                return await processor.Render(config, cancellationSource.Token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException) {
                renderError = ex;
                throw;
            }
        }

        private static ILatexProcessor CreateProcessor()
        {
            var processor = new LatexCharCreator();
            var processorWithCache = new LatexProcessingCache(processor);
            var processorWithQueueAndCache = new LatexProcessingQueue(processorWithCache);
            return processorWithQueueAndCache;
        }
    }
}
