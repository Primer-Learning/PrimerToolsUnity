using System.Threading;
using System.Threading.Tasks;

namespace Primer.Latex
{
    abstract internal class ProcessingPipeline : ILatexProcessor
    {
        protected readonly ILatexProcessor processor;

        public LatexProcessingState state => processor.state;

        protected ProcessingPipeline(ILatexProcessor innerProcessor)
        {
            processor = innerProcessor;
        }


        public abstract Task<LatexChar[]> Process(LatexInput config, CancellationToken cancellationToken = default);


#if UNITY_EDITOR
        public void OpenBuildDir() => processor.OpenBuildDir();
#endif

    }
}
