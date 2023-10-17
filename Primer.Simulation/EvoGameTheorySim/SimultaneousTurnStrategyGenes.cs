using Primer.Simulation.Genome.Strategy;
using UnityEngine;
using System;

namespace Primer.Simulation
{
    public enum SimultaneousTurnAction
    {
        Dove,
        Hawk,
        Bully,
        Retaliator
    }
    
    public class SimultaneousTurnStrategyGenes
    {
        public class Dove : SimultaneousTurnStrategyGene
        {
            public override Color color => PrimerColor.blue;
            public override SimultaneousTurnAction action => SimultaneousTurnAction.Dove;
        }
        public class Hawk : SimultaneousTurnStrategyGene
        {
            public override Color color => PrimerColor.red;
            public override SimultaneousTurnAction action => SimultaneousTurnAction.Hawk;
        }
        public class Bully : SimultaneousTurnStrategyGene
        {
            public override Color color => PrimerColor.orange;
            public override SimultaneousTurnAction action => SimultaneousTurnAction.Bully;
        }
        public class Retaliator : SimultaneousTurnStrategyGene
        {
            public override Color color => PrimerColor.green;
            public override SimultaneousTurnAction action => SimultaneousTurnAction.Retaliator;
        }
        
        public class MixedHawkDove : SimultaneousTurnStrategyGene
        {
            float hawkChance;
            Rng rng;
            public override Color color => PrimerColor.JuicyInterpolate(PrimerColor.blue, PrimerColor.red, hawkChance);

            public override SimultaneousTurnAction action => rng.RangeFloat(1) < hawkChance
                ? SimultaneousTurnAction.Hawk
                : SimultaneousTurnAction.Dove;
            
            public MixedHawkDove(float hawkChance, Rng rng = null)
            {
                this.hawkChance = hawkChance;
                this.rng = rng;
                if (rng is null)
                {
                    Debug.LogWarning("You are using the static Rng object. If you are in a simulation, pass the simulation's Rng object.");
                }
            }

        }
    }
}