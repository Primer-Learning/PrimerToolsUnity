using System;
using System.Threading;
using System.Threading.Tasks;

namespace Primer.Latex
{
    internal class LatexProcessor : ProcessingPipeline
    {
        // Not sure if all latex rendering processes need to share the same instance or not
        // If that ever changes only this line will need to be changed
        public static LatexProcessor GetInstance() => new();


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
