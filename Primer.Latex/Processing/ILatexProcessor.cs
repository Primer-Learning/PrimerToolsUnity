using System.Threading;
using System.Threading.Tasks;

namespace Primer.Latex
{
    public interface ILatexProcessor
    {
        LatexProcessingState state { get; }

        Task<LatexChar[]> Render(LatexInput config, CancellationToken cancellationToken = default);

#if UNITY_EDITOR
        void OpenBuildDir();
#endif
    }
}
