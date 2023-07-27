using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Primer.Simulation;

namespace Simulation.GameTheory
{
    public abstract class StrategyRule<T> : IRule
    {
        public abstract void OnAgentCreated(Agent<T> agent);

        public abstract UniTask Resolve(IEnumerable<Agent<T>> agents, FruitTree tree);
    }
}
