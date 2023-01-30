using System;
using System.Threading;
using System.Threading.Tasks;

namespace Primer.Latex
{
    internal class LatexProcessor : ProcessingPipeline
    {
        private static CancellableLatexProcessor CreateInnerProcessor()
        {
            var processor = new ExpressionCreator();
            var withCache = new LatexProcessingCache(processor);
            var andQueue = new LatexProcessingQueue(withCache);
            var andCancellable = new CancellableLatexProcessor(andQueue);
            return andCancellable;
        }

        // Not sure if all latex rendering processes need to share the same instance or not
        // If that ever changes only this line will need to be changed
        // The difference would be if they share the same queue or not
        // Cache is internally stored in a static dictionary so it's shared anyway
        public static LatexProcessor GetInstance() => new();


        public LatexProcessor() : base(CreateInnerProcessor()) {}

        public override async Task<LatexExpression> Process(LatexInput config,
            CancellationToken cancellationToken = default)
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
