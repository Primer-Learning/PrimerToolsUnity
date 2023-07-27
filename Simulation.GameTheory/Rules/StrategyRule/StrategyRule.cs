using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Primer.Simulation;

namespace Simulation.GameTheory
{
    public abstract class StrategyRule<T> : IRule where T : Enum
    {
        public abstract void OnAgentCreated(Agent agent);

        public abstract UniTask Resolve(IEnumerable<Agent> agents, FruitTree tree);
    }
}
