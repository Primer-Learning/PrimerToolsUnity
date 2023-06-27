using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Primer.Simulation
{
    public static class UniTaskExtensions
    {
        public static UniTask RunInParallel(this IEnumerable<UniTask> tasks) => UniTask.WhenAll(tasks);
    }
}
