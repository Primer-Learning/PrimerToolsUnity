using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer;

namespace Simulation.GameTheory
{
    public class SimpleConflictResolution<T> : ConflictResolutionRule<T>
    {
        public override void OnAgentCreated(Agent<T> agent)
        {
            // noop
        }

        public override async UniTask Resolve(IEnumerable<Agent<T>> agents, FruitTree tree)
        {
            var (first, second) = agents.Shuffle().Take(2).ToList();

            await UniTask.WhenAll(
                first.Eat(tree),
                second.Eat(tree)
            );
        }
    }
}
