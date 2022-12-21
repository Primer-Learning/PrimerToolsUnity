using System.Threading;
using System.Threading.Tasks;

namespace Primer.Latex
{
    internal interface ILatexProcessor
    {
        LatexProcessingState state { get; }

        Task<LatexChar[]> Process(LatexInput config, CancellationToken cancellationToken = default);

#if UNITY_EDITOR
        void OpenBuildDir();
#endif
    }
}
