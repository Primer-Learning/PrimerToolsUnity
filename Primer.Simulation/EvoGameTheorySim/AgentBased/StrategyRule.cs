using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Primer.Animation;
using Primer.Simulation;

namespace Simulation.GameTheory
{
    public abstract class StrategyRule<T> : IRule where T : Enum
    {
        public RewardMatrix<T> rewardMatrix { get; set; }
        public abstract void OnAgentCreated(Agent agent);

        public abstract Tween Resolve(IEnumerable<Agent> agents, FruitTree tree);
    }
}