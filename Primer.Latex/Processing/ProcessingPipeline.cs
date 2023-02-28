using System.Threading;
using System.Threading.Tasks;

namespace Primer.Latex
{
    internal abstract class ProcessingPipeline : ILatexProcessor
    {
        protected readonly ILatexProcessor processor;

        protected ProcessingPipeline(ILatexProcessor innerProcessor) => processor = innerProcessor;

        public LatexProcessingState state => processor.state;


        public abstract Task<LatexExpression> Process(LatexInput config, CancellationToken cancellationToken = default);


#if UNITY_EDITOR
        public void OpenBuildDir() => processor.OpenBuildDir();
#endif
    }
}
