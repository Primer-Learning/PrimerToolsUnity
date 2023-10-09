using Primer.Simulation.EvoGameTheorySim.AgentBased;
using UnityEngine;
using System;

namespace Primer.Simulation.Genome.Strategy
{
    public abstract class SimultaneousTurnStrategyGene : Gene
    {
        public abstract Color color { get; }
        public abstract SimultaneousTurnAction action { get; }
    }
}