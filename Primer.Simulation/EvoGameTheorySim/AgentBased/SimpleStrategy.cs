using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer;
using Primer.Animation;

namespace Simulation.GameTheory
{
    public enum SimpleStrategy
    {
        Share
    }
    public class SimpleStrategyRule : StrategyRule<SimpleStrategy>
    {
        public override void OnAgentCreated(Creature agent)
        {
            // noop
        }

        public override Tween Resolve(IEnumerable<Creature> agents, FruitTree tree)
        {
            var (first, second) = agents.Shuffle().Take(2).ToList();

            return Tween.Series(
                first.EatAnimation(tree),
                second.EatAnimation(tree)
            );
        }
    }
}
