using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer;

namespace Simulation.GameTheory
{
    public class SimpleConflictResolution : ConflictResolutionRule
    {
        public override void OnAgentCreated(Agent agent)
        {
            // noop
        }

        public override async UniTask Resolve(IEnumerable<Agent> agents, Food food)
        {
            var (first, second) = agents.Shuffle().Take(2).ToList();

            await UniTask.WhenAll(
                first.Eat(food),
                second.Eat(food)
            );
        }
    }
}
