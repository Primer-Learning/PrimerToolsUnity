using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Primer.Latex
{
    internal class LatexProcessingCache : ProcessingPipeline
    {
        private static readonly Dictionary<LatexInput, Task<LatexExpression>> cache = new();

        private static Task<LatexExpression> GetFromCache(LatexInput config)
        {
            var foundKey = cache.Keys.FirstOrDefault(x => x.Equals(config));
            return foundKey is null ? null : cache[foundKey];
        }

        public LatexProcessingCache(ILatexProcessor innerProcessor) : base(innerProcessor) {}


        public override Task<LatexExpression> Process(LatexInput config, CancellationToken cancellationToken = default)
        {
            var cached = GetFromCache(config);

            if (cached is not null)
                return cached;

            var task = processor.Process(config, cancellationToken);
            cache.Add(config, task);
            return task;
        }
    }
}
