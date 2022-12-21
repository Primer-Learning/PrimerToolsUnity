using System;
using System.Threading;
using System.Threading.Tasks;

namespace Primer.Latex
{
    internal class LatexProcessor : ProcessingPipeline
    {
        private static LatexProcessor instanceCache;

        // Not sure if all latex rendering processes need to share the same instance or not
        // If that ever changes only this line will need to be changed
        // By returning a new instance each time instead of the cached one
        public static LatexProcessor GetInstance() => instanceCache ??= new LatexProcessor();


        private static CancellableLatexProcessor CreateInnerProcessor()
        {
            var processor = new CharacterCreator();
            var withCache = new LatexProcessingCache(processor);
            var andQueue = new LatexProcessingQueue(withCache);
            var andCancellable = new CancellableLatexProcessor(andQueue);
            return andCancellable;
        }


        public LatexProcessor() : base(CreateInnerProcessor()) {}

        public override async Task<LatexChar[]> Process(LatexInput config, CancellationToken cancellationToken = default)
        {
            renderError = null;

            try {
                return await processor.Process(config, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException) {
                renderError = ex;
                throw;
            }
        }


#if UNITY_EDITOR
        public Exception renderError;

        public void Cancel() => ((CancellableLatexProcessor)processor).Cancel();
#endif
    }
}
