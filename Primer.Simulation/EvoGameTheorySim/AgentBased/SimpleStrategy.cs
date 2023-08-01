using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer;

namespace Simulation.GameTheory
{
    public enum SimpleStrategy
    {
        Share
    }
    public class SimpleStrategyRule : StrategyRule<SimpleStrategy>
    {
        public override void OnAgentCreated(Agent agent)
        {
            // noop
        }

        public override async UniTask Resolve(IEnumerable<Agent> agents, FruitTree tree)
        {
            var (first, second) = agents.Shuffle().Take(2).ToList();

            await UniTask.WhenAll(
                first.EatAnimation(tree),
                second.EatAnimation(tree)
            );
        }
    }
}
