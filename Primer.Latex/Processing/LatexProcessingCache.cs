using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Primer.Latex
{
    public class LatexProcessingCache : ILatexProcessor
    {
        private static readonly Dictionary<LatexInput, Task<LatexChar[]>> cache = new();

        private readonly ILatexProcessor processor;

        public LatexProcessingState state => processor.state;


        public LatexProcessingCache(ILatexProcessor innerProcessor)
        {
            processor = innerProcessor;
        }


        public Task<LatexChar[]> Render(LatexInput config, CancellationToken cancellationToken = default)
        {
            var cached = GetFromCache(config);
            if (cached is not null) return cached;

            var task = processor.Render(config, cancellationToken);
            cache.Add(config, task);
            return task;
        }

        private static Task<LatexChar[]> GetFromCache(LatexInput config)
        {
            var foundKey = cache.Keys.FirstOrDefault(x => x.Equals(config));
            return foundKey is null ? null : cache[foundKey];
        }

#if UNITY_EDITOR
        public void OpenBuildDir() => processor.OpenBuildDir();
#endif
    }
}
