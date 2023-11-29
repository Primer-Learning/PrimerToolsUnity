using System;
using System.Collections.Generic;
using Primer.Animation;
using Primer.Simulation;

namespace Simulation.GameTheory
{
    public abstract class SimultaneousTurnGameAgentHandler
    {
        public RewardMatrix rewardMatrix { get; set; }
        public abstract void OnAgentCreated(SimultaneousTurnCreature agent);

        public abstract IEnumerable<Tween> Resolve(IEnumerable<SimultaneousTurnCreature> agents, FruitTree tree);
    }
}
