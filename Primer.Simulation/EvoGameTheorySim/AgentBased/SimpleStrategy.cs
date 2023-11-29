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
    public class SimpleSimultaneousTurnGameAgentHandler : SimultaneousTurnGameAgentHandler
    {
        public override void OnAgentCreated(SimultaneousTurnCreature agent)
        {
            // noop
        }

        public override IEnumerable<Tween> Resolve(IEnumerable<SimultaneousTurnCreature> agents, FruitTree tree)
        {
            var (first, second) = agents.Shuffle().Take(2).ToList();

            yield return Tween.Series(
                first.EatAnimation(tree, first.OrderFruitByDistance(tree)[0]),
                second.EatAnimation(tree, second.OrderFruitByDistance(tree)[0])
            );
        }
    }
}
